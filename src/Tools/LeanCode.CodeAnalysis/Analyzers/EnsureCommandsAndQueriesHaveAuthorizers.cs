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
        private const string CommandTypeName = "LeanCode.CQRS.ICommand";
        private const string QueryTypeName = "LeanCode.CQRS.IQuery";

#pragma warning disable RS2008
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
#pragma warning restore RS2008

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(CommandRule, QueryRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        public void AnalyzeSymbol(SymbolAnalysisContext context)
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
            if (IsCommand(type))
            {
                return ContractType.Command;
            }
            else if (IsQuery(type))
            {
                return ContractType.Query;
            }
            else
            {
                return null;
            }
        }

        private static bool IsCommand(INamedTypeSymbol type)
        {
            return type.TypeKind != TypeKind.Interface && type.ImplementsInterfaceOrBaseClass(CommandTypeName) && !type.IsAbstract;
        }

        private static bool IsQuery(INamedTypeSymbol type)
        {
            return type.TypeKind != TypeKind.Interface && type.ImplementsInterfaceOrBaseClass(QueryTypeName) && !type.IsAbstract;
        }

        private enum ContractType
        {
            Query,
            Command,
        }
    }
}
