using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnsureCommandsAndQueriesHaveAuthorizers : DiagnosticAnalyzer
    {
        private const string Category = "Cqrs";
        private const string MessageFormat = @"`{0}` has no authorization attributes specified. Consider adding one or use [AllowUnauthorized] to explicitly mark no authorization";

        private static readonly DiagnosticDescriptor CommandRule = new DiagnosticDescriptor(
            DiagnosticsIds.CommandsShouldHaveAuthorizers,
            "Command should be authorized",
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor QueryRule = new DiagnosticDescriptor(
            DiagnosticsIds.QueriesShouldHaveAuthorizers,
            "Query should be authorized",
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(CommandRule, QueryRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;
            var contractType = GetContractType(type);

            if (contractType == null)
            {
                return;
            }

            if (!type.HasAuthorizationAttribute())
            {
                var rule = contractType == ContractType.Command ? CommandRule : QueryRule;
                var diagnostic = Diagnostic.Create(rule, type.Locations[0], type.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static ContractType? GetContractType(INamedTypeSymbol type)
        {
            if (type.IsCommand())
            {
                return ContractType.Command;
            }
            else if (type.IsQuery())
            {
                return ContractType.Query;
            }
            else
            {
                return null;
            }
        }

        private enum ContractType
        {
            Query,
            Command,
        }
    }
}
