using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Analyzer = LeanCode.CodeAnalysis.Analyzers.EnsureCQRSHandlersFollowNamingConvention;

namespace LeanCode.CodeAnalysis.CodeActions;

public class FixCQRSHandlerNamingCodeAction : CodeAction
{
    private readonly Document document;
    private readonly TextSpan classSpan;

    public override string Title => "Fix CQRS handler name";
    public override string EquivalenceKey => Title;

    public FixCQRSHandlerNamingCodeAction(Document document, TextSpan classSpan)
    {
        this.document = document;
        this.classSpan = classSpan;
    }

    protected override async Task<Solution?> GetChangedSolutionAsync(CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        var model = await document.GetSemanticModelAsync(cancellationToken);

        if (root is null || model is null)
        {
            return solution;
        }

        var token = root.FindToken(classSpan.Start);

        if (token.IsKind(SyntaxKind.IdentifierToken) && token.Parent is ClassDeclarationSyntax classDeclaration)
        {
            var expectedHandlerName = GetExpectedCQRSHandlerName(classDeclaration, model, cancellationToken);
            var classSymbol = model.GetDeclaredSymbol(classDeclaration, cancellationToken)!;

            return await Renamer.RenameSymbolAsync(
                solution,
                classSymbol,
                new(RenameFile: true),
                expectedHandlerName,
                cancellationToken
            );
        }
        else
        {
            return solution;
        }
    }

    private static string GetExpectedCQRSHandlerName(
        ClassDeclarationSyntax handlerDeclaration,
        SemanticModel model,
        CancellationToken cancellationToken
    )
    {
        var type = model.GetDeclaredSymbol(handlerDeclaration, cancellationToken)!;

        // If diagnostic has been reported we know that handler implements only one
        // CQRS operation so `handlerType` is not null here.
        var handlerType = Analyzer.GetCQRSHandlerType(type, out var implementationCount)!;

        return Analyzer.GetExpectedCQRSHandlerName(type, handlerType.Value, implementationCount);
    }
}
