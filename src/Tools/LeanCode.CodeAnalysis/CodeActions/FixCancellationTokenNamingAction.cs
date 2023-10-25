using LeanCode.CodeAnalysis.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LeanCode.CodeAnalysis.CodeActions;

public class FixCancellationTokenNamingAction : CodeAction
{
    private readonly Document document;
    private readonly TextSpan classSpan;

    public override string Title => $"Fix CancellationToken argument name";
    public override string EquivalenceKey => Title;

    public FixCancellationTokenNamingAction(Document document, TextSpan classSpan)
    {
        this.document = document;
        this.classSpan = classSpan;
    }

    protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        if (root is null)
        {
            return document;
        }

        var token = root.FindToken(classSpan.Start);

        if (
            token.Parent is ParameterSyntax parameter
            && parameter.Type is IdentifierNameSyntax identifierName
            && identifierName.Identifier.Text == CancellationTokensShouldFollowNamingConvention.IdentifierName
            && parameter.Identifier.Text != CancellationTokensShouldFollowNamingConvention.ExpectedName
        )
        {
            var oldIdentifierText = parameter.Identifier.Text;
            var containingMethod = parameter.FirstAncestorOrSelf<MethodDeclarationSyntax>();

            if (containingMethod != null)
            {
                // Find all references to the old parameter name replace them.
                var references = containingMethod
                    .DescendantNodes()
                    .Where(
                        node =>
                            node is IdentifierNameSyntax identifier && identifier.Identifier.Text == oldIdentifierText
                    )
                    .Append(parameter);

                // When using ReplaceNode/ReplaceNodes, the entire tree is rebuilt. To ensure that
                // the old nodes are still available, we need to replace them all at once.
                var newRoot = root.ReplaceNodes(
                    references,
                    (oldNode, newNode) =>
                    {
                        return oldNode switch
                        {
                            ParameterSyntax parameter
                                => parameter.WithIdentifier(
                                    SyntaxFactory.Identifier(
                                        CancellationTokensShouldFollowNamingConvention.ExpectedName
                                    )
                                ),
                            _
                                => SyntaxFactory.IdentifierName(
                                    CancellationTokensShouldFollowNamingConvention.ExpectedName
                                ),
                        };
                    }
                );

                return document.WithSyntaxRoot(newRoot);
            }
        }

        return document;
    }
}
