using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnsureCommandsQueriesAndOperationsHaveAuthorizers : DiagnosticAnalyzer
{
    private const string Category = "Cqrs";
    private const string MessageFormat =
        @"`{0}` has no authorization attributes specified. Consider adding one or use [AllowUnauthorized] to explicitly mark no authorization.";
    private const string CommandTypeName = "LeanCode.Contracts.ICommand";
    private const string QueryTypeName = "LeanCode.Contracts.IQuery";
    private const string OperationTypeName = "LeanCode.Contracts.IOperation";

    private static readonly DiagnosticDescriptor CommandRule =
        new(
            DiagnosticsIds.CommandsShouldHaveAuthorizers,
            "Command should be authorized",
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

    private static readonly DiagnosticDescriptor QueryRule =
        new(
            DiagnosticsIds.QueriesShouldHaveAuthorizers,
            "Query should be authorized",
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

    private static readonly DiagnosticDescriptor OperationRule =
        new(
            DiagnosticsIds.OperationsShouldHaveAuthorizers,
            "Operation should be authorized",
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(CommandRule, QueryRule, OperationRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    public static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        var contractType = GetContractType(type);

        if (contractType == null)
        {
            return;
        }

        if (!type.HasAuthorizationAttribute())
        {
            var rule = contractType switch
            {
                ContractType.Command => CommandRule,
                ContractType.Query => QueryRule,
                ContractType.Operation => OperationRule,
                _ => throw new InvalidOperationException("Invalid contract type"),
            };

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
        else if (IsOperation(type))
        {
            return ContractType.Operation;
        }
        else
        {
            return null;
        }
    }

    private static bool IsCommand(INamedTypeSymbol type)
    {
        return type.TypeKind != TypeKind.Interface
            && type.ImplementsInterfaceOrBaseClass(CommandTypeName)
            && !type.IsAbstract;
    }

    private static bool IsQuery(INamedTypeSymbol type)
    {
        return type.TypeKind != TypeKind.Interface
            && type.ImplementsInterfaceOrBaseClass(QueryTypeName)
            && !type.IsAbstract;
    }

    private static bool IsOperation(INamedTypeSymbol type)
    {
        return type.TypeKind != TypeKind.Interface
            && type.ImplementsInterfaceOrBaseClass(OperationTypeName)
            && !type.IsAbstract;
    }

    private enum ContractType
    {
        Query,
        Command,
        Operation,
    }
}
