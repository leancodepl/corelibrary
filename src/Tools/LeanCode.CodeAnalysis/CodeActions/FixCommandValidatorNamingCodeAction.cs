using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Analyzer = LeanCode.CodeAnalysis.Analyzers.EnsureCommandValidatorsFollowNamingConvention;

namespace LeanCode.CodeAnalysis.CodeActions;

public class FixCommandValidatorNamingCodeAction : CodeAction
{
    private readonly Document document;
    private readonly TextSpan classSpan;

    public override string Title => "Fix command validator name";
    public override string EquivalenceKey => Title;

    public FixCommandValidatorNamingCodeAction(Document document, TextSpan classSpan)
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
            var classSymbol = model.GetDeclaredSymbol(classDeclaration, cancellationToken)!;

            var validator = Analyzer.GetImplementedValidator(classSymbol)!;
            var expectedValidatorName = Analyzer.GetCommandValidatorExpectedName(validator);

            return await Renamer.RenameSymbolAsync(
                solution,
                classSymbol,
                new(),
                expectedValidatorName,
                cancellationToken
            );
        }
        else
        {
            return solution;
        }
    }
}
