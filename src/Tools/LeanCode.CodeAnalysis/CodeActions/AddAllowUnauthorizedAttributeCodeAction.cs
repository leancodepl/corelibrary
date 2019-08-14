using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.CQRS.Security;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LeanCode.CodeAnalysis.CodeActions
{
    public class AddAllowUnauthorizedAttributeCodeAction : CodeAction
    {
        private readonly Document document;
        private readonly TextSpan classSpan;

        public override string Title => "Add AllowUnauthorizedAttribute";
        public override string EquivalenceKey => Title;

        public AddAllowUnauthorizedAttributeCodeAction(Document document, TextSpan classSpan)
        {
            this.document = document;
            this.classSpan = classSpan;
        }

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var model = await document.GetSemanticModelAsync(cancellationToken);

            var classDeclaration = root.FindNode(classSpan).FirstAncestorOrSelf<ClassDeclarationSyntax>();

            var attribute = SF.Attribute(SF.ParseName(nameof(AllowUnauthorizedAttribute)));
            editor.AddAttribute(classDeclaration, attribute);

            // if(model.)
            // editor.AddMember

            // var info = model.GetSymbolInfo(?)

            return editor.GetChangedDocument();
        }

        public Task<Document> Test() => GetChangedDocumentAsync(CancellationToken.None);

        // private bool HasAuthorizerNamespace(SyntaxNode root)
        // {
        //     return root.DescendantNodes()
        //         .OfType<UsingDirectiveSyntax>()
        //         .Where(ns => )
        // }
    }
}
