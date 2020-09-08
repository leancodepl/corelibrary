using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace LeanCode.CodeAnalysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnsureAllValidationErrorCodesAreUsed : DiagnosticAnalyzer
    {
        private const string MessageFormat = "`{0}` does not use all validation error codes from `{1}`. Not checked codes `{2}`";

        public static readonly ImmutableHashSet<string> ValidationErrorClasses = ImmutableHashSet.Create(
            "ErrorCodes",
            "ValidationErrors");

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticsIds.AllValidationErrorCodesShouldBeUsed,
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
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;
            if (!type.IsValidator())
            {
                return;
            }

            var diags = type.DeclaringSyntaxReferences
                .Select(s => GetTypeNodesAndModel(s, context))
                .SelectMany(x => x.Nodes.Select(n => x.Model.GetOperation(n, context.CancellationToken)))
                .Where(op => op != null)
                .OfType<IFieldReferenceOperation>()
                .Select(op => op.Field)
                .Where(IsErrorCodeField)
                .ToLookup(op => (ITypeSymbol)op.ContainingSymbol)
                .Select(cl =>
                {
                    var unused = GetAllClassErrorCodes(cl.Key, context).Except(cl).Select(f => f.Name).ToList();
                    if (unused.Any())
                    {
                        return Diagnostic.Create(Rule, type.Locations[0], type.Name, cl.Key.Name, string.Join(", ", unused));
                    }
                    else
                    {
                        return null;
                    }
                })
                .Where(d => d != null);

            foreach (var diag in diags)
            {
                context.ReportDiagnostic(diag);
            }
        }

        private static IEnumerable<IFieldSymbol> GetAllClassErrorCodes(ITypeSymbol @class, SymbolAnalysisContext context)
        {
            var intType = context.Compilation.GetSpecialType(SpecialType.System_Int32);

            return @class
                .DeclaringSyntaxReferences
                .Select(s => GetTypeNodesAndModel(s, context))
                .SelectMany(x => x.Nodes.OfType<FieldDeclarationSyntax>()
                    .SelectMany(f => ProcessField(f, x.Model)));

            IEnumerable<IFieldSymbol> ProcessField(FieldDeclarationSyntax s, SemanticModel model)
            {
                return s
                    .Declaration
                    .Variables
                    .Select(v => (IFieldSymbol)model.GetDeclaredSymbol(v, context.CancellationToken))
                    .Where(f =>
                        SymbolEqualityComparer.Default.Equals(f.Type, intType) &&
                        f.DeclaredAccessibility == Accessibility.Public &&
                        f.IsStaticReadonlyOrConst());
            }
        }

        private static (SemanticModel Model, IEnumerable<SyntaxNode> Nodes) GetTypeNodesAndModel(
            SyntaxReference syntax,
            SymbolAnalysisContext context)
        {
            var model = context.Compilation.GetSemanticModel(syntax.SyntaxTree);
            var root = syntax.SyntaxTree.GetRoot(context.CancellationToken);
            var node = root.FindNode(syntax.Span);
            return (model, node.DescendantNodes());
        }

        private static bool IsErrorCodeField(IFieldSymbol field)
        {
            return ValidationErrorClasses.Contains(field.ContainingSymbol.Name)
                && field.IsStaticReadonlyOrConst();
        }
    }
}
