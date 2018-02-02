using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;


namespace LeanCode.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnsureCommandsHaveAuthorizers : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "LNCD0001"; // :)

        private const string Category = "Cqrs";
        private const string Title = "Command should be authorized";
        private const string MessageFormat = @"`{0}` has no authorization attributes specified. Consider adding one or use [AllowUnauthorized] to explicitly mark no authorization";
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        private const string CommandTypeName = "LeanCode.CQRS.ICommand";
        private const string AuthorizeWhenTypeName = "LeanCode.CQRS.Security.AuthorizeWhenAttribute";
        private const string AllowUnauthorizedTypeName = "LeanCode.CQRS.Security.AllowAnauthorizedAttribute";

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;
            if (IsCommand(type) && !HasAuthorizationAttribute(type))
            {
                var diagnostic = Diagnostic.Create(Rule, type.Locations[0], type.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsCommand(INamedTypeSymbol type)
        {
            return type.ImplementsInterfaceOrBaseClass(CommandTypeName);
        }

        private static bool HasAuthorizationAttribute(INamedTypeSymbol type)
        {
            var attributes = type.GetAttributes();
            return attributes.Any(attr =>
                attr.AttributeClass.ImplementsInterfaceOrBaseClass(AuthorizeWhenTypeName) ||
                attr.AttributeClass.ImplementsInterfaceOrBaseClass(AllowUnauthorizedTypeName));
        }
    }
}
