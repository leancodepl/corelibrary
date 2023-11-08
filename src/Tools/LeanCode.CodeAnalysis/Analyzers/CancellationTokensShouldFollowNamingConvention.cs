using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CancellationTokensShouldFollowNamingConvention : DiagnosticAnalyzer
{
    internal const string TypeName = "CancellationToken";
    internal const string ParameterName = "cancellationToken";

    private static readonly DiagnosticDescriptor Rule =
        new(
            DiagnosticsIds.CancellationTokensShouldFollowNamingConvention,
            "CancellationTokens should follow naming convention",
            "`{0}` should follow `{1}` naming convention",
            "Naming",
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
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        // Skip analyzing methods with 'override' or 'new' keywords as they might come from external source
        if (
            context.Node is MethodDeclarationSyntax methodDeclaration
            && !methodDeclaration
                .Modifiers
                .Any(modifier => modifier.IsKind(SyntaxKind.OverrideKeyword) || modifier.IsKind(SyntaxKind.NewKeyword))
        )
        {
            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                if (
                    parameter.Type is IdentifierNameSyntax identifierName
                    && identifierName.Identifier.Text == TypeName
                    && parameter.Identifier.Text != ParameterName
                )
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        parameter.Identifier.GetLocation(),
                        parameter.Identifier.Text,
                        ParameterName
                    );

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
