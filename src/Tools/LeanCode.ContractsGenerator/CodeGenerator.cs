using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Configuration;
using LeanCode.CQRS;
using System.Globalization;

namespace LeanCode.ContractsGenerator
{
    class RemoteQueryCommandInfo
    {
        public string Name { get; set; }
        public string Parameter { get; set; }
        public string Result { get; set; }
        public bool IsQuery { get; set; }
        public string Path { get; set; }
    }

    public class CodeGenerator
    {
        private readonly List<SyntaxTree> trees;
        private readonly CSharpCompilation compilation;

        private readonly string contractsPreamble;
        private readonly string clientPreamble;
        private readonly string name;
        private readonly Dictionary<string, string> typeTranslations;

        List<RemoteQueryCommandInfo> remoteQueryCommandsInfo;

        public CodeGenerator(List<SyntaxTree> trees, CSharpCompilation compilation, GeneratorConfiguration configuration)
        {
            this.trees = trees;
            this.compilation = compilation;

            contractsPreamble = configuration.ContractsPreamble;
            clientPreamble = configuration.ClientPreamble;
            name = configuration.Name;
            typeTranslations = configuration.TypeTranslations;

            this.remoteQueryCommandsInfo = new List<RemoteQueryCommandInfo>();
        }

        public void Generate(out string contracts, out string client)
        {
            StringBuilder dtosBuilder = new StringBuilder();
            StringBuilder funcsBuilder = new StringBuilder();

            dtosBuilder.Append(contractsPreamble);
            dtosBuilder.Append("\n\n");

            var namespaceStatement = new NamespaceStatement
            {
                Name = name
            };

            foreach (var tree in trees)
            {
                var model = compilation.GetSemanticModel(tree);

                GenerateClassesAndInterfaces(model, tree, namespaceStatement);
                GenerateEnums(model, tree, namespaceStatement);
            }

            namespaceStatement.Render(dtosBuilder, 0);

            funcsBuilder.Append(clientPreamble);

            funcsBuilder.Append("export type ClientParams = {\n");
            funcsBuilder.Append(
                string.Join(",\n", remoteQueryCommandsInfo
                    .Select(info => $"    \"{info.Name}\": {info.Parameter}")
                )
            );
            funcsBuilder.Append("\n};\n\n");

            funcsBuilder.Append("export type ClientResults = {\n");
            funcsBuilder.Append(
                string.Join(",\n", remoteQueryCommandsInfo
                    .Select(info => $"    \"{info.Name}\": {info.Result}")
                )
            );
            funcsBuilder.Append("\n};\n\n");

            funcsBuilder.Append("export default function (cqrsClient: CQRS): ClientType<ClientParams, ClientResults> {\n");
            funcsBuilder.Append("    return {\n");

            funcsBuilder.Append(
                string.Join(",\n", remoteQueryCommandsInfo
                    .Select(info =>
                    {
                        var executeOption = info.IsQuery ? "executeQuery" : "executeCommand";

                        return $"        {info.Name}: cqrsClient.{executeOption}.bind(cqrsClient, \"{info.Path}\")";
                    })
                )
            );
            funcsBuilder.Append("\n    };\n");
            funcsBuilder.Append("}\n");

            contracts = dtosBuilder.ToString();
            client = funcsBuilder.ToString();
        }

        private void GenerateRemoteCommand(INamedTypeSymbol info)
        {
            var name = Char.ToLower(info.Name[0]) + info.Name.Substring(1);
            var namespaceName = GetFullNamespaceName(info.ContainingNamespace);

            remoteQueryCommandsInfo.Add(new RemoteQueryCommandInfo
            {
                Name = name,
                Path = $"{namespaceName}.{info.Name}",
                Parameter = $"{this.name}.{info.Name}",
                Result = "CommandResult",
                IsQuery = false
            });
        }

        private void GenerateRemoteQuery(INamedTypeSymbol info)
        {
            var name = Char.ToLower(info.Name[0]) + info.Name.Substring(1);
            var result = info.AllInterfaces.Where(i => i.Name == "IRemoteQuery").Select(q => StringifyType(q.TypeArguments.Skip(1).First(), out _)).First();
            var namespaceName = GetFullNamespaceName(info.ContainingNamespace);

            remoteQueryCommandsInfo.Add(new RemoteQueryCommandInfo
            {
                Name = name,
                Path = $"{namespaceName}.{info.Name}",
                Parameter = $"{this.name}.{info.Name}",
                Result = $"{this.name}.{result}",
                IsQuery = true
            });
        }

        private InterfaceStatement GenerateInterface(INamedTypeSymbol info)
        {
            if (info.AllInterfaces.Any(a => a.Name == "_Attribute" || a.Name == "_Exception"))
            {
                return null;
            }

            var baseTypes = info.Interfaces.Select(i => StringifyType(i, out _)).Where(t => t != "ValueType").ToList();

            if (info.BaseType != null && info.BaseType.Name != "Object")
            {
                var baseType = StringifyType(info.BaseType, out _);
                if (baseType != "ValueType")
                {
                    baseTypes.Add(baseType);
                }
            }

            var consts = info.GetMembers().OfType<IFieldSymbol>()
                .Where(i => i.HasConstantValue)
                .Select(c => new ConstStatement
                {
                    Name = c.Name,
                    Value = StringifyBuiltInTypeValue(c.ConstantValue)
                })
                .ToList();

            var children = info.GetMembers().OfType<INamedTypeSymbol>().Select(GenerateInterface);

            return new InterfaceStatement
            {
                Name = info.Name,
                IsStatic = info.IsStatic,
                Arguments = info.TypeParameters.Select(ParseTypeArgument).ToList(),
                Extends = baseTypes,
                Fields = GenerateProperties(info).ToList(),
                Constants = consts,
                Children = children.ToList()
        };
        }

        private IEnumerable<FieldStatement> GenerateProperties(INamedTypeSymbol info)
        {
            var properties = info.GetMembers().OfType<IPropertySymbol>();

            foreach (var property in properties)
            {
                if (property.DeclaredAccessibility.HasFlag(Accessibility.Public))
                {
                    var type = StringifyType(property.Type, out bool isNullable);
                    var nullable = isNullable || HasCanBeNullAttribute(property);

                    yield return new FieldStatement
                    {
                        Name = property.Name,
                        IsOptional = nullable,
                        Type = type
                    };
                }
            }
        }

        private static bool HasCanBeNullAttribute(IPropertySymbol typeSymbol)
        {
            return typeSymbol.GetAttributes().Any(attr => attr.AttributeClass.Name == typeof(CanBeNullAttribute).Name);
        }

        private void GenerateClassesAndInterfaces(SemanticModel model, SyntaxTree tree, NamespaceStatement namespaceStatement)
        {
            var classesDeclarations = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var interfacesDeclarations = tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            var structsDeclarations = tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>();

            var classes = classesDeclarations.Select(c => model.GetDeclaredSymbol(c)).Union(structsDeclarations.Select(s => model.GetDeclaredSymbol(s))).ToList();
            var interfaces = interfacesDeclarations.Select(i => model.GetDeclaredSymbol(i)).ToList();

            var publicClasses = classes.Where(i => i.DeclaredAccessibility.HasFlag(Accessibility.Public)).ToList();
            var publicInterfaces = interfaces.Where(i => i.DeclaredAccessibility.HasFlag(Accessibility.Public)).ToList();

            var rootLevelClasses = publicClasses.Where(i => i.ContainingType == null).ToList();
            var publicNonAbstractClasses = publicClasses.Where(c => !c.IsAbstract).ToList();

            foreach (var info in publicNonAbstractClasses.Where(IsRemoteCommand))
            {
                GenerateRemoteCommand(info);
            }

            foreach (var info in publicNonAbstractClasses.Where(IsRemoteQuery))
            {
                GenerateRemoteQuery(info);
            }

            foreach (var info in rootLevelClasses.Concat(publicInterfaces).Where(i => !IsCommandOrQuery(i) || IsRemoteCommandOrQuery(i)))
            {
                namespaceStatement.Children.Add(GenerateInterface(info));
            }
        }

        private static string StringifyBuiltInTypeValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            switch (value)
            {
                case bool v:
                    return v.ToString().ToLower();

                case string v:
                    return '"' + v + '"';

                case char v:
                    return '"' + v.ToString() + '"';

                case float v:
                    return v.ToString(CultureInfo.InvariantCulture);

                case double v:
                    return v.ToString(CultureInfo.InvariantCulture);

                case decimal v:
                    return v.ToString(CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }

        private EnumStatement GenerateEnum(INamedTypeSymbol info)
        {
            var enumMembers = info.GetMembers().OfType<IFieldSymbol>();
            var enumValues = enumMembers.Select(e => new EnumValueStatement
            {
                Name = e.Name,
                Value = StringifyBuiltInTypeValue(e.ConstantValue)
            }).ToList();

            return new EnumStatement
            {
                Name = info.Name,
                Values = enumValues
            };
        }

        private void GenerateEnums(SemanticModel model, SyntaxTree tree, NamespaceStatement namespaceStatement)
        {
            var enums = tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>();

            foreach (var info in enums.Select(e => model.GetDeclaredSymbol(e)))
            {
                namespaceStatement.Children.Add(GenerateEnum(info));
            }
        }

        private string ParseTypeArgument(ITypeParameterSymbol info)
        {
            var constraints = string.Empty;
            if (info.ConstraintTypes.Any())
            {
                constraints = " extends ";
                constraints += string.Join(" & ", info.ConstraintTypes.Select(t => StringifyType(t, out _)).Where(t => t != "ValueType"));
            }
            return $"{info.Name}{constraints}";
        }

        private string StringifyType(ITypeSymbol typeSymbol, out bool isNullable)
        {
            isNullable = typeSymbol.Name == "Nullable";

            switch (typeSymbol)
            {
                case INamedTypeSymbol type:
                    if (isNullable)
                    {
                        return StringifyType(type.TypeArguments.First(), out _);
                    }
                    if (type.AllInterfaces.Any(i => i.Name == "IDictionary") && type.Arity >= 2)
                    {
                        return $"{{ [index: {(StringifyType(type.TypeArguments.First(), out _))}]: {(StringifyType(type.TypeArguments.Last(), out _))} }}";
                    }
                    if (type.AllInterfaces.Any(i => i.Name == "IEnumerable") && type.Arity >= 1)
                    {
                        return $"{(StringifyType(type.TypeArguments.First(), out _))}[]";
                    }
                    if (type.Arity > 0)
                    {
                        return type.Name + "<" + string.Join(", ", type.TypeArguments.Select(t => StringifyType(t, out _))) + ">";
                    }
                    if (typeTranslations.TryGetValue(type.Name.ToLower(), out string name))
                    {
                        return name;
                    }
                    return type.Name;

                case IArrayTypeSymbol type:
                    return $"{(StringifyType(type.ElementType, out _))}[]";

                case ITypeParameterSymbol type:
                    return type.Name;

                case IDynamicTypeSymbol type:
                    return "any";

                default: throw new Exception("Unknown type.");
            }
        }

        private static string GetFullNamespaceName(INamespaceSymbol info)
        {
            if (info.ContainingNamespace.IsGlobalNamespace)
            {
                return info.Name;
            }
            return GetFullNamespaceName(info.ContainingNamespace) + "." + info.Name;
        }

        private static bool IsCommandOrQuery(INamedTypeSymbol info)
        {
            return info.AllInterfaces.Any(i => i.Name == "IQuery" || i.Name == "ICommand");
        }

        private static bool IsRemoteCommandOrQuery(INamedTypeSymbol info)
        {
            return IsRemoteCommand(info) || IsRemoteQuery(info);
        }

        private static bool IsRemoteCommand(INamedTypeSymbol info)
        {
            return info.AllInterfaces.Any(i => i.Name == "IRemoteCommand");
        }

        private static bool IsRemoteQuery(INamedTypeSymbol info)
        {
            return info.AllInterfaces.Any(i => i.Name == "IRemoteQuery");
        }
    }
}
