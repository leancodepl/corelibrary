using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LeanCode.DomainModels.Generators;

[Generator]
public sealed class TypedIdGenerator : IIncrementalGenerator
{
    private const string AttributeName = "LeanCode.DomainModels.Ids.TypedIdAttribute";
    private const string CustomPrefixField = "CustomPrefix";
    private const string CustomGeneratorField = "CustomGenerator";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var src = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeName,
            static (n, _) =>
                n is RecordDeclarationSyntax
                && n.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.RecordStructDeclaration),
            static (n, _) =>
            {
                var attribute = n.Attributes.First(a => a.AttributeClass?.Name == "TypedIdAttribute");
                return new TypedIdData(
                    (TypedIdFormat)
                        Convert.ToInt32(attribute.ConstructorArguments.First().Value!, CultureInfo.InvariantCulture),
                    n.TargetSymbol.ContainingNamespace.ToDisplayString(),
                    n.TargetSymbol.Name,
                    (string?)attribute.NamedArguments.FirstOrDefault(a => a.Key == CustomPrefixField).Value.Value,
                    (string?)attribute.NamedArguments.FirstOrDefault(a => a.Key == CustomGeneratorField).Value.Value
                );
            }
        );

        context.RegisterSourceOutput(
            src,
            static (sources, typedId) => sources.AddSource($"{typedId.TypeName}.g.cs", IdSource.Build(typedId))
        );
    }
}
