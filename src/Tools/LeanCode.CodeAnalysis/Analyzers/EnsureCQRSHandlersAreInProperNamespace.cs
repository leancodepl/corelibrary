using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnsureCQRSHandlersAreInProperNamespace : DiagnosticAnalyzer
{
    internal const string ContractsNamepsacePart = "Contracts";
    internal const string HandlersNamespacePart = "CQRS";
    private static readonly string[] CqrsHandlerTypes =
    [
        "LeanCode.CQRS.Execution.ICommandHandler`1",
        "LeanCode.CQRS.Execution.IQueryHandler`2",
        "LeanCode.CQRS.Execution.IOperationHandler`2"
    ];

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticsIds.CQRSHandlersShouldBeInProperNamespace,
        "CQRS handlers should be in proper namespace",
        "CQRS handler's namespace '{0}' does not match the contract's namespace '{1}'",
        "Cqrs",
        DiagnosticSeverity.Warning,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.RegisterSyntaxNodeAction(
            AnalyzeSymbol,
            SyntaxKind.FileScopedNamespaceDeclaration,
            SyntaxKind.NamespaceDeclaration
        );
    }

    private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var namespaceSymbol = (INamespaceSymbol)context.ContainingSymbol!;
        var syntaxTree = context.Node.SyntaxTree;

        var (expectedNamespace, contractNamespace) = GetNamespacesForHandler(namespaceSymbol);

        if (expectedNamespace != null && contractNamespace != null && namespaceSymbol.ToString() != expectedNamespace)
        {
            // Necessary to add warning only to the whole line with namespace.
            var namespaceDeclaration = GetNamespaceDeclaration(context, namespaceSymbol);
            var lineSpan = namespaceDeclaration!.GetLocation().GetLineSpan();
            var textSpan = syntaxTree.GetText().Lines[lineSpan.StartLinePosition.Line].Span;

            var diagnostic = Diagnostic.Create(
                Rule,
                Location.Create(syntaxTree, textSpan),
                namespaceSymbol.ToString(),
                contractNamespace
            );

            context.ReportDiagnostic(diagnostic);
        }
    }

    internal static (string? expectedNamespace, string? contractNamespace) GetNamespacesForHandler(
        INamespaceSymbol namespaceSymbol
    )
    {
        string? expectedNamespace = null;
        string? contractNamespace = null;

        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            var implementedContracts = type.AllInterfaces
                .Where(i => CqrsHandlerTypes.Contains(i.GetFullNamespaceName()))
                .Select(i => i.TypeArguments.First());

            foreach (var c in implementedContracts)
            {
                contractNamespace = c.ContainingNamespace.ToString();
                var handlerNamespace = type.ContainingNamespace.ToString();

                var contractsSubStringIdx = LastIndexDotPrefixedSuffixedOrStartEnd(
                    contractNamespace,
                    ContractsNamepsacePart
                );
                var cqrsSubStringIdx = LastIndexDotPrefixedSuffixedOrStartEnd(handlerNamespace, HandlersNamespacePart);

                // Return null if contract or handler does not have `.Contracts.` or `.Contracts`
                // or `.CQRS.` or`.CQRS` part in their namespace.
                if (contractsSubStringIdx == -1 || cqrsSubStringIdx == -1)
                {
                    return (null, null);
                }

                var contractSuffix = contractNamespace[(contractsSubStringIdx + ContractsNamepsacePart.Length)..];
                var handlerPrefix = handlerNamespace[..(cqrsSubStringIdx + HandlersNamespacePart.Length)];
                var currentExpectedNamespace = handlerPrefix + contractSuffix;

                if (expectedNamespace is null)
                {
                    expectedNamespace = currentExpectedNamespace;
                }
                // Return null if we have handlers which implement contracts
                // in different namespaces.
                else if (expectedNamespace != currentExpectedNamespace)
                {
                    return (null, null);
                }
            }
        }

        return (expectedNamespace, contractNamespace);
    }

    /// <summary>
    /// Returns the index of the <c>substring</c> if <c>value</c> is equal
    /// to <c>substring</c>, starts with <c>substring.</c>,
    /// contains <c>.substring.</c>, or ends with <c>.substring</c>.
    /// </summary>
    private static int LastIndexDotPrefixedSuffixedOrStartEnd(string value, string substring)
    {
        if (value == substring)
        {
            return 0;
        }

        if (value.StartsWith(substring + ".", StringComparison.InvariantCulture))
        {
            return 0;
        }

        if (value.EndsWith("." + substring, StringComparison.InvariantCulture))
        {
            return value.Length - substring.Length;
        }

        if (value.Contains("." + substring + ".", StringComparison.InvariantCulture))
        {
            var index = value.LastIndexOf("." + substring + ".", StringComparison.InvariantCulture);

            if (index != -1)
            {
                return index + 1;
            }
        }

        return -1;
    }

    private static BaseNamespaceDeclarationSyntax? GetNamespaceDeclaration(
        SyntaxNodeAnalysisContext context,
        INamespaceSymbol namespaceSymbol
    )
    {
        var root = context.Node.SyntaxTree.GetRoot();
        var semanticModel = context.SemanticModel;

        foreach (var node in root.DescendantNodes())
        {
            if (
                node is BaseNamespaceDeclarationSyntax namespaceDeclaration
                && SymbolEqualityComparer
                    .Default
                    .Equals(semanticModel.GetDeclaredSymbol(namespaceDeclaration), namespaceSymbol)
            )
            {
                return namespaceDeclaration;
            }
        }

        return null;
    }
}
