using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnsureQueriesHaveAuthorizers : DiagnosticAnalyzer
    {
        private const string Category = "Cqrs";
        private const string Title = "Query should be authorized";
        private const string MessageFormat = @"`{0}` has no authorization attributes specified. Consider adding one or use [AllowUnauthorized] to explicitly mark no authorization";
        private const string QueryTypeName = "LeanCode.CQRS.IQuery";
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticsIds.QueriesShouldHaveAuthorizers, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        public void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;
            if (IsQuery(type) && !type.HasAuthorizationAttribute())
            {
                var diagnostic = Diagnostic.Create(Rule, type.Locations[0], type.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsQuery(INamedTypeSymbol type)
        {
            return type.TypeKind != TypeKind.Interface && type.ImplementsInterfaceOrBaseClass(QueryTypeName) && !type.IsAbstract;
        }
    }
}
