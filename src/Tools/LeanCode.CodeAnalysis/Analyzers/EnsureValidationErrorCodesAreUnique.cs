using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnsureValidationErrorCodesAreUnique : DiagnosticAnalyzer
    {
        private const string MessageFormat = "`{0}` error code value is the same as in `{1}`.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticsIds.ErrorCodesShouldBeUnique,
            "All validation error codes should be used",
            MessageFormat,
            "Cqrs",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        private void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            var diags = context
                .Compilation
                .SyntaxTrees
                .Select(t => t.GetRoot(context.CancellationToken))
                .SelectMany(t => t.DescendantNodes())
                .OfType<ClassDeclarationSyntax>()
                .Select(c =>
                {
                    var model = context.Compilation.GetSemanticModel(c.SyntaxTree);
                    var type = (INamedTypeSymbol)model.GetDeclaredSymbol(c, context.CancellationToken);
                    return (Class: c, Model: model, Type: type);
                })
                .Where(c => IsErrorCodesClass(c.Type))
                .SelectMany(c => GetAllClassErrorCodes(c.Class, c.Model, context.CancellationToken))
                .ToLookup(c => c.Value, c => c.Field)
                .Where(c => c.Count() > 1)
                .Select(c =>
                {
                    static string FormatName(IFieldSymbol f) => $"{f.ContainingType.GetFullNamespaceName()}.{f.MetadataName}";
                    var first = c.First();
                    var other = c.Skip(1);

                    var otherFields = string.Join(", ", other.Select(FormatName));

                    return Diagnostic.Create(Rule, first.Locations[0], FormatName(first), otherFields);
                });

            foreach (var diag in diags)
            {
                context.ReportDiagnostic(diag);
            }
        }

        private static IEnumerable<(int Value, IFieldSymbol Field)> GetAllClassErrorCodes(
            ClassDeclarationSyntax @class,
            SemanticModel model,
            CancellationToken cancellationToken)
        {
            var intType = model.Compilation.GetSpecialType(SpecialType.System_Int32);

            return @class
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .SelectMany(s => ProcessField(s, model));

            IEnumerable<(int Value, IFieldSymbol Field)> ProcessField(FieldDeclarationSyntax s, SemanticModel model)
            {
                return s
                    .Declaration
                    .Variables
                    .Select(v => (Node: v, Symbol: (IFieldSymbol)model.GetDeclaredSymbol(v, cancellationToken)))
                    .Where(f =>
                        SymbolEqualityComparer.Default.Equals(f.Symbol.Type, intType) &&
                        f.Symbol.DeclaredAccessibility == Accessibility.Public &&
                        f.Symbol.IsStaticReadonlyOrConst())
                    .Select(f => (f.Symbol, Value: TryGetValue(f.Node, model, cancellationToken)))
                    .Where(v => v.Value != null)
                    .Select(v => ((int)v.Value!, v.Symbol));
            }
        }

        private static int? TryGetValue(VariableDeclaratorSyntax variable, SemanticModel model, CancellationToken cancellationToken)
        {
            var init = variable.Initializer?.Value;
            return init is null ? null :
                model.GetConstantValue(init, cancellationToken) is { HasValue: true, Value: var value } ? (int?)value : null;
        }

        private static bool IsErrorCodesClass(INamedTypeSymbol symbol)
        {
            return EnsureAllValidationErrorCodesAreUsed.ValidationErrorClasses.Contains(symbol.Name) &&
                (!symbol?.ContainingType.IsCommand() ?? true);
        }
    }
}
