using System.Collections.Immutable;
using System.Composition;
using LeanCode.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace LeanCode.CodeAnalysis.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FixCQRSHandlerNamespaceCodeFixProvider))]
[Shared]
public class FixCQRSHandlerNamespaceCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(DiagnosticsIds.CQRSHandlersShouldBeInProperNamespace);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.RegisterCodeFix(
            new FixCQRSHandlerNamespaceCodeAction(context.Document, context.Span),
            context.Diagnostics
        );

        return Task.CompletedTask;
    }
}
