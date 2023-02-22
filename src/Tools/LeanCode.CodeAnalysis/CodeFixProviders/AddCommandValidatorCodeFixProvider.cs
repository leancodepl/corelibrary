using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace LeanCode.CodeAnalysis.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddAuthorizationAttributeCodeFixProvider))]
[Shared]
public class AddCommandValidatorCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(DiagnosticsIds.CommandsShouldHaveValidators);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.RegisterCodeFix(new AddCommandValidatorCodeAction(context.Document, context.Span), context.Diagnostics);

        return Task.CompletedTask;
    }
}
