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

        public CodeGenerator(List<SyntaxTree> trees, CSharpCompilation compilation, GeneratorConfiguration configuration)
        {
            this.trees = trees;
            this.compilation = compilation;

            contractsPreamble = configuration.ContractsPreamble;
            clientPreamble = configuration.ClientPreamble;
            name = configuration.Name;
            typeTranslations = configuration.TypeTranslations;
        }

        public void Generate(out string contracts, out string client)
        {
            var clientStatement = new ClientStatement
            {
                Name = name
            };

            foreach (var tree in trees)
            {
                var model = compilation.GetSemanticModel(tree);

                var interfaces = GenerateClassesAndInterfaces(model, tree).ToList();
                var enums = GenerateEnums(model, tree);

                clientStatement.Children.AddRange(enums);
                clientStatement.Children.AddRange(interfaces);
            }

            StringBuilder dtosBuilder = new StringBuilder().Append(contractsPreamble).Append("\n\n");
            StringBuilder funcsBuilder = new StringBuilder().Append(clientPreamble);

            clientStatement.Render(dtosBuilder, funcsBuilder);

            contracts = dtosBuilder.ToString();
            client = funcsBuilder.ToString();
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

            var interfaceStatement = new InterfaceStatement
            {
                Name = info.Name,
                IsStatic = info.IsStatic,
                Arguments = info.TypeParameters.Select(ParseTypeArgument).ToList(),
                Extends = baseTypes,
                Fields = GenerateProperties(info).ToList(),
                Constants = consts,
                Children = children.ToList(),
            };

            if (!info.IsAbstract)
            {
                if (IsRemoteCommand(info))
                {
                    return new CommandStatement(interfaceStatement)
                    {
                        NamespaceName = GetFullNamespaceName(info.ContainingNamespace)
                    };
                }
                else if (IsRemoteQuery(info))
                {
                    return new QueryStatement(interfaceStatement)
                    {
                        NamespaceName = GetFullNamespaceName(info.ContainingNamespace)
                    };
                }
            }

            return interfaceStatement;
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

        private IEnumerable<InterfaceStatement> GenerateClassesAndInterfaces(SemanticModel model, SyntaxTree tree)
        {
            var classesDeclarations = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var interfacesDeclarations = tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            var structsDeclarations = tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>();

            var classes = classesDeclarations.Select(c => model.GetDeclaredSymbol(c)).Union(structsDeclarations.Select(s => model.GetDeclaredSymbol(s))).ToList();
            var interfaces = interfacesDeclarations.Select(i => model.GetDeclaredSymbol(i)).ToList();

            var publicClasses = classes.Where(i => i.DeclaredAccessibility.HasFlag(Accessibility.Public)).ToList();
            var publicInterfaces = interfaces.Where(i => i.DeclaredAccessibility.HasFlag(Accessibility.Public)).ToList();

            var rootLevelClasses = publicClasses.Where(i => i.ContainingType == null).ToList();

            foreach (var info in rootLevelClasses.Concat(publicInterfaces).Where(i => !IsCommandOrQuery(i) || IsRemoteCommandOrQuery(i)))
            {
                yield return GenerateInterface(info);
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

        private IEnumerable<EnumStatement> GenerateEnums(SemanticModel model, SyntaxTree tree)
        {
            var enums = tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>();

            foreach (var info in enums.Select(e => model.GetDeclaredSymbol(e)))
            {
                yield return GenerateEnum(info);
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
