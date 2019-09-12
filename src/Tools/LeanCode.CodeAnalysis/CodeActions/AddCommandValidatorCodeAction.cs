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

namespace LeanCode.CodeAnalysis.CodeActions
{
    public class AddCommandValidatorCodeAction : CodeAction
    {
        private const string HandlerFullTypeName = "LeanCode.CQRS.Execution.ICommandHandler`2";
        private const string ValidatorType = "ContextualValidator";
        private const string ValidatorNamespace = "LeanCode.CQRS.Validation.Fluent";

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

            var handlerSyntax = root.FindNode(handlerSpan).FirstAncestorOrSelf<ClassDeclarationSyntax>();
            var concreteHandler = editor.SemanticModel.GetDeclaredSymbol(handlerSyntax) as INamedTypeSymbol;
            var handlerInteface = concreteHandler.AllInterfaces
                .FirstOrDefault(i => i.GetFullNamespaceName() == HandlerFullTypeName);

            var commandName = handlerInteface.TypeArguments[1].Name;
            var indentation = handlerSyntax.GetLeadingTrivia();

            var (validator, baseValidatorName) = BuildValidator(commandName, indentation);

            editor.InsertBefore(handlerSyntax, validator);

            if (!Helpers.TypeIsResolvable(editor.SemanticModel, handlerSyntax.SpanStart, baseValidatorName))
            {
                Helpers.InsertNamespaceDirective(editor, root, ValidatorNamespace);
            }

            return editor.GetChangedDocument();
        }

        protected static (ClassDeclarationSyntax, NameSyntax) BuildValidator(string commandName, SyntaxTriviaList indentation)
        {
            var name = commandName + "CV";

            var baseValidatorName = SF.GenericName(ValidatorType)
                                .WithTypeArgumentList(
                                    SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(
                                        SF.IdentifierName(commandName))));

            // Don't do it alone
            // https://roslynquoter.azurewebsites.net/
            var validator = SF.ClassDeclaration(name)
                .WithAdditionalAnnotations(Formatter.Annotation)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    SF.BaseList(
                        SF.SingletonSeparatedList<BaseTypeSyntax>(
                            SF.SimpleBaseType(baseValidatorName))))
                .WithMembers(
                    SF.SingletonList<MemberDeclarationSyntax>(
                        SF.ConstructorDeclaration(SF.Identifier(name))
                            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                            .WithBody(SF.Block())))
                .NormalizeWhitespace(eol: "\n")
                // .WithLeadingTrivia(indentation)
                .WithTrailingTrivia(SF.ParseTrailingTrivia("\n\n"));

            return (validator, baseValidatorName);
        }
    }
}
