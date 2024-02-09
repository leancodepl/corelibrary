using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SuggestCommandsHaveValidators : DiagnosticAnalyzer
{
    private const string HandlerTypeName = "LeanCode.CQRS.Execution.ICommandHandler`1";
    private const string ValidatorTypeName = "FluentValidation.IValidator`1";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticsIds.CommandsShouldHaveValidators,
        "Commands should be validated",
        "`{0}` command should be validated",
        "Cqrs",
        DiagnosticSeverity.Info,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.ContainingSymbol!;

        if (!IsCommandHandler(type, out var commandType))
        {
            return;
        }

        var tree = type.DeclaringSyntaxReferences.First().SyntaxTree;

        if (!CommandIsValidated(commandType, tree, context.SemanticModel))
        {
            var diagnostic = Diagnostic.Create(Rule, type.Locations[0], commandType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsCommandHandler(INamedTypeSymbol type, [NotNullWhen(true)] out INamedTypeSymbol? commandType)
    {
        var handler = type.AllInterfaces.FirstOrDefault(i => i.GetFullNamespaceName() == HandlerTypeName);

        if (handler is null)
        {
            commandType = null;
            return false;
        }

        var typeArgs = handler.TypeArguments;
        if (typeArgs is [INamedTypeSymbol cmdType])
        {
            commandType = cmdType;
            return true;
        }

        commandType = null;
        return false;
    }

    private static bool CommandIsValidated(INamedTypeSymbol command, SyntaxTree tree, SemanticModel model)
    {
        // This way of checking is a hack, we are looking for validators only in current file.
        // However searching for implementations e.g. via SymbolFinder requires access to full solution
        // which we don't have here.
        var classes = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
        var types = classes.Select(cl => model.GetDeclaredSymbol(cl) as ITypeSymbol);
        return types.Where(cl => cl != null).Where(cl => IsThisCommandValidator(command, cl!)).Any();
    }

    private static bool IsThisCommandValidator(INamedTypeSymbol command, ITypeSymbol type)
    {
        return type
            .AllInterfaces.Where(i => i.GetFullNamespaceName() == ValidatorTypeName)
            .Where(i => SymbolEqualityComparer.Default.Equals(i.TypeArguments.First(), command))
            .Any();
    }
}
