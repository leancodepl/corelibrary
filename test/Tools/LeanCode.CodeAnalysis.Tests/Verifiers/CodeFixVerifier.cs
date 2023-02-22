using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Verifiers;

public abstract class CodeFixVerifier : DiagnosticVerifier
{
    protected abstract CodeFixProvider GetCodeFixProvider();

    protected async Task VerifyCodeFix(string oldSource, string newSource, string[] expectedFixes, int? fixToApply = null, bool allowNewCompilerDiagnostics = false)
    {
        oldSource = oldSource.Trim();
        newSource = newSource.Trim();

        var analyzer = GetDiagnosticAnalyzer();
        var codeFixProvider = GetCodeFixProvider();

        var document = CreateDocument(oldSource);
        var analyzerDiagnostics = await GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });
        var compilerDiagnostics = await GetCompilerDiagnostics(document);

        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
        await codeFixProvider.RegisterCodeFixesAsync(context);

        actions.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.Ordinal));

        var actualFixTitles = actions.Select(f => f.Title);
        Assert.Equal(expectedFixes, actualFixTitles);

        if (fixToApply != null)
        {
            document = await ApplyFix(document, actions[(int)fixToApply]);
        }

        var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnostics(document));

        if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
        {
            document = document.WithSyntaxRoot(Formatter.Format(await document.GetSyntaxRootAsync(), Formatter.Annotation, document.Project.Solution.Workspace));
            newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnostics(document));

            throw new Xunit.Sdk.XunitException(
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Fix introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
                    string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString())),
                    (await document.GetSyntaxRootAsync()).ToFullString()));
        }

        var actual = await GetStringFromDocument(document);
        Assert.Equal(newSource, actual);
    }

    private static async Task<Document> ApplyFix(Document document, CodeAction codeAction)
    {
        var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
        var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        return solution.GetDocument(document.Id);
    }

    private static IEnumerable<Diagnostic> GetNewDiagnostics(IEnumerable<Diagnostic> diagnostics, IEnumerable<Diagnostic> newDiagnostics)
    {
        var oldArray = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        var newArray = newDiagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();

        var oldIndex = 0;
        var newIndex = 0;

        while (newIndex < newArray.Length)
        {
            if (oldIndex < oldArray.Length && oldArray[oldIndex].Id == newArray[newIndex].Id)
            {
                ++oldIndex;
                ++newIndex;
            }
            else
            {
                yield return newArray[newIndex++];
            }
        }
    }

    private static async Task<IEnumerable<Diagnostic>> GetCompilerDiagnostics(Document document)
    {
        var model = await document.GetSemanticModelAsync();
        return model.GetDiagnostics();
    }

    private static async Task<string> GetStringFromDocument(Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        return root.GetText().ToString();
    }
}
