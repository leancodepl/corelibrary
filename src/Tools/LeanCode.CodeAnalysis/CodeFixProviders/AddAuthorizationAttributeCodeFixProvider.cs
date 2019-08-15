using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.CodeActions;
using LeanCode.CQRS.Security;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace LeanCode.CodeAnalysis.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddAuthorizationAttributeCodeFixProvider))]
    [Shared]
    public class AddAuthorizationAttributeCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(
                DiagnosticsIds.QueriesShouldHaveAuthorizers,
                DiagnosticsIds.CommandsShouldHaveAuthorizers);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var (type, ns) in StaticAuthorizers)
            {
                context.RegisterCodeFix(
                    new AddAuthorizationAttributeCodeAction(context.Document, context.Span, type, ns),
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }

        public override FixAllProvider GetFixAllProvider() => null;

        private static readonly (string Type, string Namespace)[] StaticAuthorizers = new[]
        {
            (nameof(AllowUnauthorizedAttribute), "LeanCode.CQRS.Security"),
            (nameof(AuthorizeWhenHasAnyOfAttribute), "LeanCode.CQRS.Security"),
        };
    }
}
