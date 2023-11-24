using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Analyzer = LeanCode.CodeAnalysis.Analyzers.EnsureCQRSHandlersAreInProperNamespace;

namespace LeanCode.CodeAnalysis.CodeActions;

public class FixCQRSHandlerNamespaceCodeAction : CodeAction
{
    private readonly Document document;
    private readonly TextSpan classSpan;

    public override string Title => "Fix CQRS handler namespace";
    public override string EquivalenceKey => Title;

    public FixCQRSHandlerNamespaceCodeAction(Document document, TextSpan classSpan)
    {
        this.document = document;
        this.classSpan = classSpan;
    }

    protected override async Task<Solution?> GetChangedSolutionAsync(CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        var model = await document.GetSemanticModelAsync(cancellationToken);

        if (root is null || model is null)
        {
            return solution;
        }

        var token = root.FindToken(classSpan.Start);

        if (token.Parent is BaseNamespaceDeclarationSyntax namespaceDeclaration)
        {
            var namespaceSymbol = (INamespaceSymbol)model.GetDeclaredSymbol(namespaceDeclaration, cancellationToken)!;
            var currentNamespace = namespaceSymbol.ToString();

            var (expectedNamespace, _) = Analyzer.GetNamespacesForHandler(namespaceSymbol);

            if (expectedNamespace is null)
            {
                return solution;
            }

            var newNamespaceIdentifier = SyntaxFactory.ParseName(expectedNamespace);

            var newNamespaceDeclaration = namespaceDeclaration
                .WithName(newNamespaceIdentifier)
                .WithLeadingTrivia(namespaceDeclaration.GetLeadingTrivia())
                .WithTrailingTrivia(namespaceDeclaration.GetTrailingTrivia());

            var newRoot = root.ReplaceNode(namespaceDeclaration, newNamespaceDeclaration);
            var newSolution = solution.WithDocumentSyntaxRoot(document.Id, newRoot);

            var updatedDocument = newSolution.GetDocument(document.Id);

            await MoveDocumentToContractMatchingFolderIfNecessaryAsync(
                updatedDocument,
                currentNamespace,
                expectedNamespace,
                cancellationToken
            );

            return newSolution;
        }
        else
        {
            return solution;
        }
    }

    private static async Task MoveDocumentToContractMatchingFolderIfNecessaryAsync(
        Document? document,
        string currentNamespace,
        string expectedNamespace,
        CancellationToken cancellationToken
    )
    {
        if (document != null && document.FilePath != null)
        {
            var fileName = Path.GetFileName(document.FilePath);
            var projectName = document.Project.AssemblyName;
            var currentDirectory = Path.GetDirectoryName(document.FilePath);

            var currDirectoryIdx = currentDirectory.LastIndexOf(projectName, StringComparison.InvariantCulture);
            var currNamespaceFirstIdx = Analyzer.LastIndexDotPrefixedSuffixedOrStartEnd(currentNamespace, projectName);
            var currNamespaceLastIdx = Analyzer.LastIndexDotPrefixedSuffixedOrStartEnd(
                currentNamespace,
                Analyzer.HandlersNamespacePart
            );
            var expectedNamespaceIdx = Analyzer.LastIndexDotPrefixedSuffixedOrStartEnd(
                expectedNamespace,
                Analyzer.HandlersNamespacePart
            );

            if (
                currDirectoryIdx != -1
                && currNamespaceFirstIdx != -1
                && currNamespaceLastIdx != -1
                && expectedNamespaceIdx != -1
            )
            {
                currDirectoryIdx = currDirectoryIdx + projectName.Length;
                currNamespaceFirstIdx = currNamespaceFirstIdx + projectName.Length;

                // Path prefix with project name.
                var pathPrefix = currentDirectory[..currDirectoryIdx];
                // Namespace without project name prefix and CQRS suffix.
                var currentMiddlePart = currentNamespace[currNamespaceFirstIdx..currNamespaceLastIdx];
                // Namespace suffix starting with CQRS.
                var namespaceSuffix = expectedNamespace[expectedNamespaceIdx..];

                // `Path.Combine` will return the second argument if it begins with a separation character.
                var pathMiddlePart = currentMiddlePart
                    .Replace('.', Path.DirectorySeparatorChar)
                    .Trim(Path.DirectorySeparatorChar);
                var pathSuffix = namespaceSuffix
                    .Replace('.', Path.DirectorySeparatorChar)
                    .Trim(Path.DirectorySeparatorChar);

                var newPath = Path.Combine(pathPrefix, pathMiddlePart, pathSuffix, fileName);

                if (newPath != document.FilePath)
                {
                    var directoryPath = Path.GetDirectoryName(newPath);

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var updatedText = (await document.GetTextAsync(cancellationToken)).ToString();
                    await File.WriteAllTextAsync(newPath, updatedText, cancellationToken);
                    File.Delete(document.FilePath);
                }
            }
        }
    }
}
