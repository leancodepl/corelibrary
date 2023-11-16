using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnsureCommandValidatorsFollowNamingConvention : DiagnosticAnalyzer
{
    private const string ValidatorTypeName = "FluentValidation.IValidator`1";
    private const string CommandTypeName = "LeanCode.Contracts.ICommand";
    private const string ExpectedSuffix = "CV";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticsIds.CommandValidatorsShouldFollowNamingConvention,
        "Validators should follow naming convention",
        "`{0}` does not follow `{1}` naming convention",
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
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.ContainingSymbol!;

        if (TryGetCommandValidator(type, out var commandValidator))
        {
            var expectedName = GetCommandValidatorExpectedName(commandValidator);

            if (type.Name != expectedName)
            {
                var diagnostic = Diagnostic.Create(Rule, type.Locations[0], type.Name, expectedName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    internal static string GetCommandValidatorExpectedName(INamedTypeSymbol commandValidator)
    {
        return commandValidator.TypeArguments.First().Name + ExpectedSuffix;
    }

    internal static INamedTypeSymbol? GetImplementedValidator(INamedTypeSymbol type)
    {
        return type.AllInterfaces.FirstOrDefault(
            interfaceSymbol => interfaceSymbol.GetFullNamespaceName() == ValidatorTypeName
        );
    }

    private static bool TryGetCommandValidator(
        INamedTypeSymbol type,
        [NotNullWhen(true)] out INamedTypeSymbol? commandValidator
    )
    {
        var validator = GetImplementedValidator(type);

        var isCommandValidator = validator
            ?.TypeArguments
            .First()
            .AllInterfaces
            .Any(i => i.GetFullNamespaceName() == CommandTypeName);

        commandValidator = isCommandValidator == true ? validator : null;

        return type.TypeKind != TypeKind.Interface && !type.IsAbstract && commandValidator != null;
    }
}
