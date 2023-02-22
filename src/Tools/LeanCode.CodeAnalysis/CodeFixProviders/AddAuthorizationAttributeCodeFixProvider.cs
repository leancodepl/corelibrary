using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.FindSymbols;

namespace LeanCode.CodeAnalysis.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddAuthorizationAttributeCodeFixProvider))]
[Shared]
public class AddAuthorizationAttributeCodeFixProvider : CodeFixProvider
{
    private const string AuthorizeWhenAttribute = "LeanCode.Contracts.Security.AuthorizeWhenAttribute";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(
            DiagnosticsIds.QueriesShouldHaveAuthorizers,
            DiagnosticsIds.CommandsShouldHaveAuthorizers,
            DiagnosticsIds.OperationsShouldHaveAuthorizers
        );

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var model = await context.Document.GetSemanticModelAsync();
        var solutionAuthorizers = await GetAvailableAuthorizersAsync(
            context.Document.Project.Solution,
            model!.Compilation
        );

        foreach (var (type, ns) in StaticAuthorizers.Concat(solutionAuthorizers).Distinct())
        {
            context.RegisterCodeFix(
                new AddAuthorizationAttributeCodeAction(context.Document, context.Span, type, ns),
                context.Diagnostics
            );
        }
    }

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    private static async Task<IEnumerable<(string Type, string Namespace)>> GetAvailableAuthorizersAsync(
        Solution solution,
        Compilation compilation
    )
    {
        var baseAttribute = compilation.GetTypeByMetadataName(AuthorizeWhenAttribute);
        var availableAttributes = await SymbolFinder.FindDerivedClassesAsync(baseAttribute, solution);
        return availableAttributes
            .Where(attr => !attr.IsAbstract)
            .Select(attr => (attr.Name, attr.ContainingNamespace.ToString()));
    }

    private static readonly (string Type, string Namespace)[] StaticAuthorizers = new[]
    {
        ("AllowUnauthorizedAttribute", "LeanCode.Contracts.Security"),
        ("AuthorizeWhenHasAnyOfAttribute", "LeanCode.Contracts.Security"),
    };
}
