using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeActions;
using LeanCode.CQRS.Security;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.FindSymbols;

namespace LeanCode.CodeAnalysis.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddAuthorizationAttributeCodeFixProvider))]
    [Shared]
    public class AddAuthorizationAttributeCodeFixProvider : CodeFixProvider
    {
        private const string AuthorizeWhenAttribute = "LeanCode.CQRS.Security.AuthorizeWhenAttribute";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(
                DiagnosticsIds.QueriesShouldHaveAuthorizers,
                DiagnosticsIds.CommandsShouldHaveAuthorizers);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var model = await context.Document.GetSemanticModelAsync();
            var solutionAuthorizers = await GetAvailableAuthorizers(context.Document.Project.Solution, model!.Compilation);

            foreach (var (type, ns) in StaticAuthorizers.Concat(solutionAuthorizers))
            {
                context.RegisterCodeFix(
                    new AddAuthorizationAttributeCodeAction(context.Document, context.Span, type, ns),
                    context.Diagnostics);
            }
        }

        private async Task<IEnumerable<(string Type, string Namespace)>> GetAvailableAuthorizers(Solution solution, Compilation compilation)
        {
            var baseAttribute = compilation.GetTypeByMetadataName(AuthorizeWhenAttribute);
            var availableAttributes = await SymbolFinder.FindDerivedClassesAsync(baseAttribute, solution);
            return availableAttributes
                .Select(attr => (attr.Name, attr.GetFullNamespaceName()));
        }

        public override FixAllProvider GetFixAllProvider() => null!;

        private static readonly (string Type, string Namespace)[] StaticAuthorizers = new[]
        {
            (nameof(AllowUnauthorizedAttribute), "LeanCode.CQRS.Security"),
            (nameof(AuthorizeWhenHasAnyOfAttribute), "LeanCode.CQRS.Security"),
        };
    }
}
