using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using LeanCode.CQRS;
using System.Globalization;
using LeanCode.ContractsGenerator.Statements;
using LeanCode.ContractsGenerator.Languages;
using LeanCode.ContractsGenerator.Languages.TypeScript;
using LeanCode.ContractsGenerator.Languages.Dart;

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

        public CodeGenerator(List<SyntaxTree> trees, CSharpCompilation compilation)
        {
            this.trees = trees;
            this.compilation = compilation;
        }

        public IEnumerable<LanguageFileOutput> Generate(GeneratorConfiguration configuration)
        {
            var clientStatement = new ClientStatement
            {
                Name = configuration.Name
            };

            foreach (var tree in trees)
            {
                var model = compilation.GetSemanticModel(tree);

                var interfaces = GenerateClassesAndInterfaces(model, tree).ToList();
                var enums = GenerateEnums(model, tree);

                clientStatement.Children.AddRange(enums);
                clientStatement.Children.AddRange(interfaces);
            }

            if (configuration.TypeScript != null)
            {
                var visitor = new TypeScriptVisitor(configuration.TypeScript);

                foreach (var outputFile in visitor.Visit(clientStatement))
                {
                    yield return outputFile;
                }
            }

            if (configuration.Dart != null)
            {
                var visitor = new DartVisitor(configuration.Dart);

                foreach (var outputFile in visitor.Visit(clientStatement))
                {
                    yield return outputFile;
                }
            }
        }

        private InterfaceStatement GenerateInterface(INamedTypeSymbol info)
        {
            if (info.AllInterfaces.Any(a => a.Name == "_Attribute" || a.Name == "_Exception"))
            {
                return null;
            }

            var baseTypes = info.Interfaces.Select(ConvertType).Where(t => t.Name != "ValueType").ToList();

            if (info.BaseType != null && info.BaseType.Name != "Object")
            {
                var baseType = ConvertType(info.BaseType);
                if (baseType.Name != "ValueType")
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
                Namespace = info.ContainingNamespace.ToString(),
                IsStatic = info.IsStatic,
                Parameters = info.TypeParameters.Select(ParseTypeArgument).ToList(),
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
                    var type = ConvertType(property.Type);
                    if (HasCanBeNullAttribute(property))
                    {
                        type.IsNullable = true;
                    }

                    yield return new FieldStatement
                    {
                        Name = property.Name,
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
                Namespace = info.ContainingNamespace.ToString(),
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

        private TypeParameterStatement ParseTypeArgument(ITypeParameterSymbol info)
        {
            return new TypeParameterStatement
            {
                Name = info.Name,
                Constraints = info.ConstraintTypes.Select(ConvertType).Where(t => t.Name != "ValueType").ToList()
            };
        }

        private TypeStatement ConvertType(ITypeSymbol typeSymbol)
        {
            bool isNullable = typeSymbol.Name == "Nullable";

            switch (typeSymbol)
            {
                case INamedTypeSymbol type:
                    if (isNullable)
                    {
                        var t = ConvertType(type.TypeArguments.First());
                        t.IsNullable = true;
                        return t;
                    }
                    if (type.AllInterfaces.Any(i => i.Name == "IDictionary") && type.Arity >= 2)
                    {
                        return new TypeStatement
                        {
                            Name = type.Name,
                            IsDictionary = true,
                            TypeArguments = type.TypeArguments.Select(ConvertType).ToList()
                        };
                    }
                    if (type.AllInterfaces.Any(i => i.Name == "IEnumerable") && type.Arity >= 1)
                    {
                        return new TypeStatement
                        {
                            Name = type.Name,
                            IsArrayLike = true,
                            TypeArguments = type.TypeArguments.Select(ConvertType).ToList()
                        };
                    }
                    if (type.Arity > 0)
                    {
                        return new TypeStatement
                        {
                            Name = type.Name,
                            TypeArguments = type.TypeArguments.Select(ConvertType).ToList()
                        };
                    }
                    return new TypeStatement
                    {
                        Name = type.Name
                    };

                case IArrayTypeSymbol type:
                    return new TypeStatement
                    {
                        Name = type.Name,
                        IsArrayLike = true,
                        TypeArguments = new List<TypeStatement> { ConvertType(type.ElementType) }
                    };

                case ITypeParameterSymbol type:
                    return new TypeStatement
                    {
                        Name = type.Name
                    };

                case IDynamicTypeSymbol type:
                    return new TypeStatement
                    {
                        Name = "dynamic"
                    };

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
