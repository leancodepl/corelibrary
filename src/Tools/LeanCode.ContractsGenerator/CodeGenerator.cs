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
        private static readonly Dictionary<string, string> TypeTranslations = new Dictionary<string, string>
        {
            { "int", "number" },
            { "double", "number" },
            { "float", "number" },
            { "single", "number" },
            { "int32", "number" },
            { "uint32", "number" },
            { "byte", "number" },
            { "sbyte", "number" },
            { "int64", "number" },
            { "short", "number" },
            { "long", "number" },
            { "decimal", "number" },
            { "bool", "boolean" },
            { "boolean", "boolean" },
            { "datetime", "string" },
            { "timespan", "string" },
            { "datetimeoffset", "string"},
            { "guid", "string" },
            { "string", "string" },
            { "jobject", "any" },
            { "dynamic", "any" },
            { "object", "any" }
        };

        private readonly List<SyntaxTree> trees;
        private readonly CSharpCompilation compilation;
        private readonly string contractsPreamble;
        private readonly string clientPreamble;
        private readonly string name;

        List<RemoteQueryCommandInfo> remoteQueryCommandsInfo;

        public CodeGenerator(List<SyntaxTree> trees, CSharpCompilation compilation, GeneratorConfiguration configuration)
        {
            this.trees = trees;
            this.compilation = compilation;
            contractsPreamble = configuration.ContractsPreamble;
            clientPreamble = configuration.ClientPreamble;
            name = configuration.Name;

            this.remoteQueryCommandsInfo = new List<RemoteQueryCommandInfo>();
        }

        public void Generate(out string contracts, out string client)
        {
            StringBuilder dtosBuilder = new StringBuilder();
            StringBuilder funcsBuilder = new StringBuilder();

            dtosBuilder.Append(contractsPreamble);
            dtosBuilder.Append("\n\n");

            dtosBuilder.Append($"declare namespace {name} {{\n");

            foreach (var tree in trees)
            {
                var model = compilation.GetSemanticModel(tree);

                GenerateClassesAndInterfaces(dtosBuilder, model, tree);
                GenerateEnums(dtosBuilder, model, tree);
            }

            dtosBuilder.Append("}\n");


            funcsBuilder.Append(clientPreamble);
            funcsBuilder.Append("\n");

            foreach (var tree in trees)
            {
                var model = compilation.GetSemanticModel(tree);

                GenerateConstants(funcsBuilder, model, tree);
            }

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

        private void GenerateInterface(StringBuilder dtosBuilder, INamedTypeSymbol info)
        {
            if (info.AllInterfaces.Any(a => a.Name == "_Attribute" || a.Name == "_Exception"))
            {
                return;
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
            var extensions = baseTypes.Any() ? $"extends {(string.Join(", ", baseTypes))} " : string.Empty;
            var typeArguments = info.TypeParameters.Any() ? $"<{(string.Join(", ", info.TypeParameters.Select(ParseTypeArgument)))}>" : string.Empty;

            dtosBuilder.Append($"    interface {info.Name}{typeArguments} {extensions}{{\n");

            GenerateProperties(dtosBuilder, info);

            dtosBuilder.Append("    }\n\n");
        }

        private void GenerateProperties(StringBuilder dtosBuilder, INamedTypeSymbol info)
        {
            var properties = info.GetMembers().OfType<IPropertySymbol>();

            foreach (var property in properties)
            {
                if (property.DeclaredAccessibility.HasFlag(Accessibility.Public))
                {
                    var type = StringifyType(property.Type, out bool isNullable);
                    var nullable = (isNullable || HasCanBeNullAttribute(property)) ? "?" : string.Empty;

                    dtosBuilder.Append($"        {property.Name}{nullable}: {type};\n");
                }
            }
        }

        private static bool HasCanBeNullAttribute(IPropertySymbol typeSymbol)
        {
            return typeSymbol.GetAttributes().Any(attr => attr.AttributeClass.Name == typeof(CanBeNullAttribute).Name);
        }

        private void GenerateFields(StringBuilder dtosBuilder, INamedTypeSymbol info)
        {
            var fields = info.GetMembers().OfType<IFieldSymbol>();

            foreach (var field in fields)
            {
                if (!field.IsConst && field.DeclaredAccessibility.HasFlag(Accessibility.Public))
                {
                    var type = StringifyType(field.Type, out bool isNullable);
                    var nullable = isNullable ? "?" : string.Empty;

                    dtosBuilder.Append($"        {field.Name}{nullable}: {type};\n");
                }
            }
        }

        private void GenerateModule(StringBuilder dtosBuilder, INamedTypeSymbol info, List<INamedTypeSymbol> infos)
        {
            var fields = info.GetMembers().OfType<IFieldSymbol>().Where(f => f.DeclaredAccessibility.HasFlag(Accessibility.Public) && f.HasConstantValue).ToList();
            var childInfos = infos.Where(i => i.ContainingSymbol == info).ToList();
            var isStatic = info.IsStatic;

            if (isStatic || fields.Any() || childInfos.Any())
            {
                dtosBuilder.Append($"export module {info.Name} {{\n");

                foreach (var field in fields)
                {
                    dtosBuilder.Append($"    export const {field.Name} = {(field.ConstantValue is string ? '"' + field.ConstantValue.ToString() + '"' : field.ConstantValue)};\n");
                }

                foreach (var childInfo in childInfos)
                {
                    GenerateModule(dtosBuilder, childInfo, infos);
                }

                foreach (var childInfo in childInfos.Where(i => !i.IsStatic && !IsCommandOrQuery(i) || IsRemoteCommandOrQuery(i)))
                {
                    GenerateInterface(dtosBuilder, childInfo);
                }

                dtosBuilder.Append("}\n");
            }
        }

        private void GenerateClassesAndInterfaces(StringBuilder dtosBuilder, SemanticModel model, SyntaxTree tree)
        {
            var classesDeclarations = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var interfacesDeclarations = tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            var structsDeclarations = tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>();

            var classes = classesDeclarations.Select(c => model.GetDeclaredSymbol(c)).Union(structsDeclarations.Select(s => model.GetDeclaredSymbol(s))).ToList();
            var interfaces = interfacesDeclarations.Select(i => model.GetDeclaredSymbol(i)).ToList();

            var publicClasses = classes.Where(i => i.DeclaredAccessibility.HasFlag(Accessibility.Public)).ToList();
            var publicInterfaces = interfaces.Where(i => i.DeclaredAccessibility.HasFlag(Accessibility.Public)).ToList();

            var nonStaticClasses = publicClasses.Where(i => !i.IsStatic).ToList();
            var publicNonAbstractClasses = publicClasses.Where(c => !c.IsAbstract).ToList();

            foreach (var info in publicNonAbstractClasses.Where(IsRemoteCommand))
            {
                GenerateRemoteCommand(info);
            }

            foreach (var info in publicNonAbstractClasses.Where(IsRemoteQuery))
            {
                GenerateRemoteQuery(info);
            }

            foreach (var info in nonStaticClasses.Concat(publicInterfaces).Where(i => !IsCommandOrQuery(i) || IsRemoteCommandOrQuery(i)))
            {
                GenerateInterface(dtosBuilder, info);
            }
        }

        private void GenerateConstants(StringBuilder dtosBuilder, SemanticModel model, SyntaxTree tree)
        {
            var classesDeclarations = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var staticClasses = classesDeclarations
                .Select(c => model.GetDeclaredSymbol(c))
                .Where(i => i.DeclaredAccessibility.HasFlag(Accessibility.Public) && i.IsStatic)
                .ToList();

            foreach (var info in staticClasses)
            {
                GenerateConstantsFromStaticClass(dtosBuilder, info);
            }
        }

        private void GenerateConstantsFromStaticClass(StringBuilder dtosBuilder, INamedTypeSymbol info)
        {
            var constFields = info.GetMembers().OfType<IFieldSymbol>().Where(i => i.HasConstantValue).ToList();
            if (constFields.Count == 0)
            {
                return;
            }

            var hasContainingType = info.ContainingType != null;
            var outermostNamespace = hasContainingType ? info.ContainingType.Name : info.Name;
            var fieldsDepth = hasContainingType ? 2 : 1;

            dtosBuilder.Append($"export namespace {outermostNamespace} {{\n");
            if (hasContainingType)
            {
                dtosBuilder.Append($"    export namespace {info.Name} {{\n");
            }
            foreach (var field in constFields)
            {
                dtosBuilder.Append($"{GetSpacesForDepth(fieldsDepth)}export const {field.Name} = {StringifyBuiltInTypeValue(field.ConstantValue)};\n");
            }
            if (hasContainingType)
            {
                dtosBuilder.Append($"    }}\n");
            }
            dtosBuilder.Append("}\n\n");
        }

        private string GetSpacesForDepth(int depth)
        {
            return string.Join("", Enumerable.Repeat(" ", 4 * depth));
        }

        private static string StringifyBuiltInTypeValue(object value)
        {
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

        private void GenerateEnum(StringBuilder dtosBuilder, INamedTypeSymbol info)
        {
            dtosBuilder.Append($"    export const enum {@info.Name} {{\n");

            var enumMembers = info.GetMembers().OfType<IFieldSymbol>();
            var enumBody = enumMembers.Select(e => $"        {e.Name}{(e.ConstantValue == null ? string.Empty : $" = {e.ConstantValue}")}");

            dtosBuilder.Append(string.Join(",\n", enumBody));
            dtosBuilder.Append("\n    }\n\n");
        }

        private void GenerateEnums(StringBuilder dtosBuilder, SemanticModel model, SyntaxTree tree)
        {
            var enums = tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>();

            foreach (var info in enums.Select(e => model.GetDeclaredSymbol(e)))
            {
                GenerateEnum(dtosBuilder, info);
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

        private static string StringifyType(ITypeSymbol typeSymbol, out bool isNullable)
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
                    if (TypeTranslations.TryGetValue(type.Name.ToLower(), out string name))
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
