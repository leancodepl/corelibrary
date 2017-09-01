using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace LeanCode.ViewRenderer.Razor.Extensions
{
    class LayoutDirectivePass : RazorEngineFeatureBase, IRazorDirectiveClassifierPass
    {
        public int Order => 1001;

        public void Execute(
            RazorCodeDocument codeDocument,
            DocumentIntermediateNode documentNode)
        {
            var @class = documentNode.FindPrimaryClass();
            var layout = documentNode
                .FindDirectiveReferences(Layout.Directive)
                .SelectMany(d => ((DirectiveIntermediateNode)d.Node).Tokens)
                .FirstOrDefault(t => t != null);

            var prop = new LayoutNode(layout?.Content);
            @class.Children.Add(prop);
        }
    }
}
