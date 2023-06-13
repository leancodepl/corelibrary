using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LeanCode.CodeAnalysis.CodeActions;

public class AddCommandValidatorCodeAction : CodeAction
{
    private const string HandlerFullTypeName = "LeanCode.CQRS.Execution.ICommandHandler`1";
    private const string ValidatorType = "AbstractValidator";
    private const string ValidatorNamespace = "FluentValidation";

    private readonly Document document;
    private readonly TextSpan handlerSpan;

    public AddCommandValidatorCodeAction(Document document, TextSpan handlerSpan)
    {
        this.document = document;
        this.handlerSpan = handlerSpan;
    }

    public override string Title => "Add CommandValidator";
    public override string EquivalenceKey => Title;

    protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        if (root == null)
        {
            return document;
        }

        var handlerSyntax =
            root.FindNode(handlerSpan).FirstAncestorOrSelf<ClassDeclarationSyntax>()
            ?? throw new InvalidOperationException("Cannot find parent class.");
        var concreteHandler = editor.SemanticModel.GetDeclaredSymbol(
            handlerSyntax,
            cancellationToken: cancellationToken
        )!;
        var handlerInteface =
            concreteHandler.AllInterfaces.FirstOrDefault(i => i.GetFullNamespaceName() == HandlerFullTypeName)
            ?? throw new InvalidOperationException("Cannot find handler interface implementation.");

        var commandName = handlerInteface.TypeArguments[0].Name;

        var (validator, baseValidatorName) = BuildValidator(commandName);

        editor.InsertBefore(handlerSyntax, validator);

        if (!Helpers.TypeIsResolvable(editor.SemanticModel, handlerSyntax.SpanStart, baseValidatorName))
        {
            Helpers.InsertNamespaceDirective(editor, root, ValidatorNamespace);
        }

        return editor.GetChangedDocument();
    }

    protected static (ClassDeclarationSyntax, NameSyntax) BuildValidator(string commandName)
    {
        var name = commandName + "CV";

        var baseValidatorName = SF.GenericName(ValidatorType)
            .WithTypeArgumentList(
                SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(commandName)))
            );

        var validator = SF.ClassDeclaration(name)
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
            .WithBaseList(SF.BaseList(SF.SingletonSeparatedList<BaseTypeSyntax>(SF.SimpleBaseType(baseValidatorName))))
            .WithMembers(
                SF.SingletonList<MemberDeclarationSyntax>(
                    SF.ConstructorDeclaration(SF.Identifier(name))
                        .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                        .WithBody(SF.Block())
                )
            );

        return (validator, baseValidatorName);
    }
}
