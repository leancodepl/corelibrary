using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LeanCode.CodeAnalysis.CodeActions;

public static class Helpers
{
    private static readonly NamespaceNameComparer NamespaceComparer = new NamespaceNameComparer();

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
            .OrderBy(n => n.Name.ToString(), NamespaceComparer)
            .ToArray();

        if (!namespaces.Any())
        {
            editor.InsertBefore(root.DescendantNodes().First(), stmt);
            return;
        }

        var toInsert = namespaces
            .SkipWhile(ns => NamespaceComparer.Compare(ns.Name.ToString(), namespaceName) < 0)
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
        var name = SF.ParseName(namespaceName).WithLeadingTrivia(SF.ParseTrailingTrivia(" "));
        return SF.UsingDirective(name).WithTrailingTrivia(SF.ParseTrailingTrivia("\n"));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1309",
        Justification = "Ordinal comparison would be incorrect here."
    )]
    private sealed class NamespaceNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var xSystem = IsSystem(x);
            var ySystem = IsSystem(y);

            if (xSystem ^ ySystem)
            {
                return xSystem ? -1 : 1;
            }
            else
            {
                return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private static bool IsSystem(string name)
        {
            return name == "System" || name.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
