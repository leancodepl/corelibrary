using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LeanCode.CodeAnalysis.CodeActions
{
    public class Helpers
    {
        public static bool TypeIsResolvable(SemanticModel model, int position, SyntaxNode type)
        {
            var info = model.GetSpeculativeTypeInfo(position, type, SpeculativeBindingOption.BindAsTypeOrNamespace);
            return (info.Type?.Kind is SymbolKind k) && k != SymbolKind.ErrorType;
        }

        public static void InsertNamespaceDirective(DocumentEditor editor, SyntaxNode root, string namespaceName)
        {
            var stmt = BuildUsing(namespaceName);
            var namespaces = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .OrderBy(n => n.Name.ToString())
                .ToArray();

            if (!namespaces.Any())
            {
                editor.InsertBefore(root.DescendantNodes().First(), stmt);
                return;
            }

            var toInsert = namespaces.SkipWhile(ns =>
                    ns.Name.ToString().CompareTo(namespaceName) < 0)
                .FirstOrDefault();

            if (toInsert != null)
            {
                editor.InsertBefore(toInsert, stmt);
            }
            else
            {
                editor.InsertAfter(namespaces.Last(), stmt);
            }
        }

        private static SyntaxNode BuildUsing(string namespaceName)
        {
            var name = SF.ParseName(namespaceName)
                .WithLeadingTrivia(SF.ParseTrailingTrivia(" "));
            return SF.UsingDirective(name)
                .WithTrailingTrivia(SF.ParseTrailingTrivia("\n"));
        }
    }
}
