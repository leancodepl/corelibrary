using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace LeanCode.CodeAnalysis.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AllowUnauthorizedCodeFixProvider))]
    [Shared]
    public class AllowUnauthorizedCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(
                DiagnosticsIds.QueriesShouldHaveAuthorizers,
                DiagnosticsIds.CommandsShouldHaveAuthorizers);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(
                new AddAllowUnauthorizedAttributeCodeAction(context.Document, context.Span),
                context.Diagnostics);

            return Task.CompletedTask;
        }

        public override FixAllProvider GetFixAllProvider() => null;
    }
}
