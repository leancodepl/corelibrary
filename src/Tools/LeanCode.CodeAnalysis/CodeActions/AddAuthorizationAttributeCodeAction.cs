using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LeanCode.CodeAnalysis.CodeActions;

public class AddAuthorizationAttributeCodeAction : CodeAction
{
    private readonly Document document;
    private readonly TextSpan classSpan;
    private readonly string authorizationAttribute;
    private readonly string authorizationAttributeNamespace;

    public override string Title => $"Add {authorizationAttribute}";
    public override string EquivalenceKey => Title;

    public AddAuthorizationAttributeCodeAction(
        Document document,
        TextSpan classSpan,
        string authorizationAttribute,
        string authorizationAttributeNamespace
    )
    {
        this.document = document;
        this.classSpan = classSpan;
        this.authorizationAttribute = authorizationAttribute;
        this.authorizationAttributeNamespace = authorizationAttributeNamespace;
    }

    protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        var classDeclaration =
            root!.FindNode(classSpan).FirstAncestorOrSelf<ClassDeclarationSyntax>()
            ?? throw new InvalidOperationException("Cannot find parent class.");

        var authorizer = SF.Attribute(SF.ParseName(StripAttributeSuffix(authorizationAttribute)));
        var list = SF.AttributeList(SF.SingletonSeparatedList(authorizer))
            .WithTrailingTrivia(SF.ParseTrailingTrivia("\n"));

        editor.AddAttribute(classDeclaration, list);

        if (!Helpers.TypeIsResolvable(editor.SemanticModel, classDeclaration.Span.Start, authorizer.Name))
        {
            Helpers.InsertNamespaceDirective(editor, root, authorizationAttributeNamespace);
        }

        return editor.GetChangedDocument();
    }

    private static string StripAttributeSuffix(string name) =>
        name.EndsWith("Attribute", StringComparison.Ordinal) ? name[..^"Attribute".Length] : name;
}
