using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnsureCQRSHandlersFollowNamingConvention : DiagnosticAnalyzer
{
    private const string Category = "Cqrs";
    private const string MessageFormat = @"`{0}` does not follow `{1}` naming convention.";

    internal const string CommandHandlerTypeName = "LeanCode.CQRS.Execution.ICommandHandler<TCommand>";
    internal const string QueryHandlerTypeName = "LeanCode.CQRS.Execution.IQueryHandler<TQuery, TResult>";
    internal const string OperationHandlerTypeName = "LeanCode.CQRS.Execution.IOperationHandler<TOperation, TResult>";
    internal const string CommandHandlerSuffix = "CH";
    internal const string QueryHandlerSuffix = "QH";
    internal const string OperationHandlerSuffix = "OH";

    private static readonly DiagnosticDescriptor CommandHandlerRule =
        new(
            DiagnosticsIds.CommandHandlersShouldFollowNamingConvention,
            "Command handlers should follow naming convention",
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

    private static readonly DiagnosticDescriptor QueryHandlerRule =
        new(
            DiagnosticsIds.QueryHandlersShouldFollowNamingConvention,
            "Query handlers should follow naming convention",
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

    private static readonly DiagnosticDescriptor OperationHandlerRule =
        new(
            DiagnosticsIds.OperationHandlersShouldFollowNamingConvention,
            "Operation handlers should follow naming convention",
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(CommandHandlerRule, QueryHandlerRule, OperationHandlerRule);

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

        var handlerType = GetCQRSHandlerType(type, out var implementationCount);

        if (handlerType == null)
        {
            return;
        }

        var expectedName = GetExpectedCQRSHandlerName(type, handlerType.Value, implementationCount);

        if (type.Name != expectedName)
        {
            var rule = GetRule(handlerType.Value);
            var diagnostic = Diagnostic.Create(rule, type.Locations[0], type.Name, expectedName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    internal static string GetExpectedCQRSHandlerName(
        INamedTypeSymbol type,
        HandlerType handlerType,
        int implementationCount
    )
    {
        var expectedSuffix = GetSuffix(handlerType);

        if (implementationCount > 1)
        {
            return type.Name.EndsWith(expectedSuffix, StringComparison.InvariantCulture)
                ? type.Name
                : type.Name + expectedSuffix;
        }
        else
        {
            var handlerTypeName = GetHandlerTypeName(handlerType);
            var implementedContract = type.AllInterfaces.First(i => i.OriginalDefinition.ToString() == handlerTypeName);
            var genericTypeArgument = implementedContract.TypeArguments.First();

            return genericTypeArgument.Name + expectedSuffix;
        }
    }

    internal static HandlerType? GetCQRSHandlerType(INamedTypeSymbol type, out int implementationCount)
    {
        implementationCount = 0;
        var cqrsHandlerCount = 0;
        HandlerType? cqrsHandlerType = null;

        if (IsHandler(type, HandlerType.Command, out var commandCount))
        {
            cqrsHandlerCount++;
            implementationCount += commandCount;
            cqrsHandlerType = HandlerType.Command;
        }

        if (IsHandler(type, HandlerType.Query, out var queryCount))
        {
            cqrsHandlerCount++;
            implementationCount += queryCount;
            cqrsHandlerType = HandlerType.Query;
        }

        if (IsHandler(type, HandlerType.Operation, out var operationCount))
        {
            cqrsHandlerCount++;
            implementationCount += operationCount;
            cqrsHandlerType = HandlerType.Operation;
        }

        return cqrsHandlerCount == 1 ? cqrsHandlerType : null;
    }

    private static bool IsHandler(INamedTypeSymbol type, HandlerType handlerType, out int implementationCount)
    {
        var handlerTypeName = GetHandlerTypeName(handlerType);
        implementationCount = type.AllInterfaces.Count(i => i.OriginalDefinition.ToString() == handlerTypeName);

        return type.TypeKind != TypeKind.Interface && implementationCount > 0 && !type.IsAbstract;
    }

    private static string GetSuffix(HandlerType handlerType)
    {
        return handlerType switch
        {
            HandlerType.Command => CommandHandlerSuffix,
            HandlerType.Query => QueryHandlerSuffix,
            HandlerType.Operation => OperationHandlerSuffix,
            _ => throw new InvalidOperationException("Invalid handler type"),
        };
    }

    private static DiagnosticDescriptor GetRule(HandlerType handlerType)
    {
        return handlerType switch
        {
            HandlerType.Command => CommandHandlerRule,
            HandlerType.Query => QueryHandlerRule,
            HandlerType.Operation => OperationHandlerRule,
            _ => throw new InvalidOperationException("Invalid handler type"),
        };
    }

    private static string GetHandlerTypeName(HandlerType handlerType)
    {
        return handlerType switch
        {
            HandlerType.Command => CommandHandlerTypeName,
            HandlerType.Query => QueryHandlerTypeName,
            HandlerType.Operation => OperationHandlerTypeName,
            _ => throw new InvalidOperationException("Invalid handler type"),
        };
    }

    internal enum HandlerType
    {
        Query,
        Command,
        Operation,
    }
}
