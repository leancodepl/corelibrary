using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LeanCode.CodeAnalysis.CodeActions
{
    public class Helpers
    {
        public static void InsertNamespaceDirective(DocumentEditor editor, SyntaxNode root, string namespaceName)
        {
            var stmt = BuildUsing(namespaceName);
            var namespaces = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToArray();
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
