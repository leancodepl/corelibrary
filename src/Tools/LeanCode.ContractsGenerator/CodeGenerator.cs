using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LeanCode.ContractsGenerator.Languages;
using LeanCode.ContractsGenerator.Languages.Dart;
using LeanCode.ContractsGenerator.Languages.TypeScript;
using LeanCode.ContractsGenerator.Statements;
using LeanCode.CQRS;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LeanCode.ContractsGenerator
{
    internal class RemoteQueryCommandInfo
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
                Name = configuration.Name,
            };

            var contracts = trees.SelectMany(tree =>
            {
                var model = compilation.GetSemanticModel(tree);
                IEnumerable<IStatement> interfaces = GenerateClassesAndInterfaces(model, tree).ToList();
                IEnumerable<IStatement> enums = GenerateEnums(model, tree);

                return interfaces.Concat(enums);
            }).OrderBy(s => s.Name);

            clientStatement.Children.AddRange(contracts);

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
                    Value = StringifyBuiltInTypeValue(c.ConstantValue),
                })
                .ToList();

            var children = info
                .GetMembers()
                .OfType<INamedTypeSymbol>()
                .Where(s => !HasAttribute<ExcludeFromContractsGenerationAttribute>(s))
                .Select(GenerateInterface);

            var interfaceStatement = new InterfaceStatement
            {
                Name = info.Name,
                Namespace = GetFullNamespaceName(info.ContainingNamespace, info.ContainingType),
                IsClass = info.BaseType != null,
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
                        Namespace = GetFullNamespaceName(info.ContainingNamespace),
                    };
                }
                else if (IsRemoteQuery(info))
                {
                    return new QueryStatement(interfaceStatement)
                    {
                        Namespace = GetFullNamespaceName(info.ContainingNamespace),
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
                    if (HasAttribute<CanBeNullAttribute>(property))
                    {
                        type.IsNullable = true;
                    }

                    yield return new FieldStatement
                    {
                        Name = property.Name,
                        Type = type,
                    };
                }
            }
        }

        private bool HasAttribute<TAttribute>(ISymbol symbol)
            where TAttribute : Attribute
        {
            return symbol
                .GetAttributes()
                .Any(attr => attr.AttributeClass.Name == typeof(TAttribute).Name);
        }

        private IEnumerable<InterfaceStatement> GenerateClassesAndInterfaces(SemanticModel model, SyntaxTree tree)
        {
            var classesDeclarations = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var interfacesDeclarations = tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            var structsDeclarations = tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>();

            var classes = classesDeclarations.Select(c => model.GetDeclaredSymbol(c)).Union(structsDeclarations.Select(s => model.GetDeclaredSymbol(s)));
            var interfaces = interfacesDeclarations.Select(i => model.GetDeclaredSymbol(i));

            var publicClasses = classes.Where(i => i.DeclaredAccessibility.HasFlag(Accessibility.Public));
            var publicInterfaces = interfaces.Where(i => i.DeclaredAccessibility.HasFlag(Accessibility.Public));

            var rootLevelClasses = publicClasses.Where(i => i.ContainingType == null);

            return rootLevelClasses
                .Concat(publicInterfaces)
                .Where(i => !HasAttribute<ExcludeFromContractsGenerationAttribute>(i))
                .Where(i => !IsCommandOrQuery(i) || IsRemoteCommandOrQuery(i))
                .Select(GenerateInterface);
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
                Value = StringifyBuiltInTypeValue(e.ConstantValue),
            }).ToList();

            return new EnumStatement
            {
                Name = info.Name,
                Namespace = info.ContainingNamespace.ToString(),
                Values = enumValues,
            };
        }

        private IEnumerable<EnumStatement> GenerateEnums(SemanticModel model, SyntaxTree tree)
        {
            var enums = tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>();
            return enums
                .Select(e => model.GetDeclaredSymbol(e))
                .Where(i => !HasAttribute<ExcludeFromContractsGenerationAttribute>(i))
                .Select(GenerateEnum);
        }

        private TypeParameterStatement ParseTypeArgument(ITypeParameterSymbol info)
        {
            return new TypeParameterStatement
            {
                Name = info.Name,
                Constraints = info.ConstraintTypes.Select(ConvertType).Where(t => t.Name != "ValueType").ToList(),
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
                    else if (type.AllInterfaces.Any(i => i.Name == "IDictionary") && type.Arity >= 2)
                    {
                        return new TypeStatement
                        {
                            Name = type.Name,
                            Namespace = GetFullNamespaceName(type.ContainingNamespace),
                            IsDictionary = true,
                            TypeArguments = type.TypeArguments.Select(ConvertType).ToList(),
                        };
                    }
                    else if (type.AllInterfaces.Any(i => i.Name == "IEnumerable") && type.Arity >= 1)
                    {
                        return new TypeStatement
                        {
                            Name = type.Name,
                            Namespace = GetFullNamespaceName(type.ContainingNamespace),
                            IsArrayLike = true,
                            TypeArguments = type.TypeArguments.Select(ConvertType).ToList(),
                        };
                    }
                    else if (type.Arity > 0)
                    {
                        return new TypeStatement
                        {
                            Name = type.Name,
                            Namespace = type is ITypeParameterSymbol ? string.Empty : GetFullNamespaceName(type.ContainingNamespace),
                            TypeArguments = type.TypeArguments.Select(ConvertType).ToList(),
                        };
                    }
                    else
                    {
                        return new TypeStatement
                        {
                            Name = type.ContainingType != null ? type.ContainingType.Name + "." : string.Empty + type.Name,
                            Namespace = GetFullNamespaceName(type.ContainingNamespace),
                        };
                    }

                case IArrayTypeSymbol type:
                    return new TypeStatement
                    {
                        Name = type.Name,
                        Namespace = GetFullNamespaceName(type.ContainingNamespace),
                        IsArrayLike = true,
                        TypeArguments = new List<TypeStatement> { ConvertType(type.ElementType) },
                    };

                case ITypeParameterSymbol type:
                    return new TypeStatement
                    {
                        Name = type.Name,
                        Namespace = GetFullNamespaceName(type.ContainingNamespace),
                    };

                case IDynamicTypeSymbol type:
                    return new TypeStatement
                    {
                        Name = "dynamic",
                    };

                default: throw new Exception("Unknown type.");
            }
        }

        private static string GetFullNamespaceName(INamespaceSymbol info, INamedTypeSymbol type = null)
        {
            if (info == null)
            {
                return string.Empty;
            }

            if (info.ContainingNamespace == null || info.ContainingNamespace.IsGlobalNamespace)
            {
                return info.Name;
            }

            var name = info.Name;

            if (type != null)
            {
                name = $"{name}.{type.Name}";
            }

            return GetFullNamespaceName(info.ContainingNamespace) + "." + name;
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
