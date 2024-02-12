using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LeanCode.DomainModels.Generators;

[Generator]
public sealed class TypedIdGenerator : IIncrementalGenerator
{
    private const string AttributeName = "LeanCode.DomainModels.Ids.TypedIdAttribute";
    private const string CustomPrefixField = "CustomPrefix";
    private const string SkipRandomGeneratorField = "SkipRandomGenerator";

    private static readonly DiagnosticDescriptor InvalidTypeRule =
        new(
            "LNCD0005",
            "Typed id must be `readonly partial record struct`",
            @"`{0}` is invalid. For typed ids to work, the type must be `readonly partial record struct`.",
            "Domain",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var src = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeName,
            static (n, _) => n is TypeDeclarationSyntax,
            static (n, _) =>
            {
                var attribute = n.Attributes.First(a => a.AttributeClass?.Name == "TypedIdAttribute");
                var idFormat = Convert.ToInt32(
                    attribute.ConstructorArguments.First().Value!,
                    CultureInfo.InvariantCulture
                );
                var customPrefix = attribute.NamedArguments.FirstOrDefault(a => a.Key == CustomPrefixField).Value.Value;
                var skipRandomGenerator = attribute
                    .NamedArguments.FirstOrDefault(a => a.Key == SkipRandomGeneratorField)
                    .Value.Value;
                var isValid = IsValidSyntaxNode(n.TargetNode);
                return new TypedIdData(
                    (TypedIdFormat)idFormat,
                    n.TargetSymbol.ContainingNamespace.ToDisplayString(),
                    n.TargetSymbol.Name,
                    (string?)customPrefix,
                    skipRandomGenerator is true,
                    isValid,
                    !isValid ? n.TargetNode.GetLocation() : null
                );
            }
        );

        context.RegisterSourceOutput(
            src,
            static (sources, data) =>
            {
                if (data.IsValid)
                {
                    sources.AddSource($"{data.TypeName}.g.cs", IdSource.Build(data));
                }
                else
                {
                    sources.ReportDiagnostic(Diagnostic.Create(InvalidTypeRule, data.Location, data.TypeName));
                }
            }
        );
    }

    private static bool IsValidSyntaxNode(SyntaxNode node)
    {
        return node is RecordDeclarationSyntax rec
            && rec.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.RecordStructDeclaration)
            && rec.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)
            && rec.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ReadOnlyKeyword);
    }
}
