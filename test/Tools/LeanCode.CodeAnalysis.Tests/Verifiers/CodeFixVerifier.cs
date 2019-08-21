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

namespace LeanCode.CodeAnalysis.Tests.Verifiers
{
    public abstract class CodeFixVerifier : DiagnosticVerifier
    {
        protected abstract CodeFixProvider GetCSharpCodeFixProvider();
        protected Task VerifyCSharpFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            return VerifyFix(GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics);
        }

        private async Task VerifyFix(DiagnosticAnalyzer analyzer, CodeFixProvider codeFixProvider, string oldSource, string newSource, int? codeFixIndex, bool allowNewCompilerDiagnostics)
        {
            var document = CreateDocument(oldSource);
            var analyzerDiagnostics = await GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });
            var compilerDiagnostics = await GetCompilerDiagnostics(document);
            var attempts = analyzerDiagnostics.Count;

            for (int i = 0; i < attempts; ++i)
            {
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
                await codeFixProvider.RegisterCodeFixesAsync(context);

                if (!actions.Any())
                {
                    break;
                }

                if (codeFixIndex != null)
                {
                    document = await ApplyFix(document, actions[(int)codeFixIndex]);
                    break;
                }

                document = await ApplyFix(document, actions.ElementAt(0));
                analyzerDiagnostics = await GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });

                var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnostics(document));

                if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
                {
                    document = document.WithSyntaxRoot(Formatter.Format(await document.GetSyntaxRootAsync(), Formatter.Annotation, document.Project.Solution.Workspace));
                    newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnostics(document));

                    throw new Xunit.Sdk.XunitException(
                        string.Format(
                            "Fix introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
                            string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString())),
                            (await document.GetSyntaxRootAsync()).ToFullString()));
                }

                if (!analyzerDiagnostics.Any())
                {
                    break;
                }
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
            var simplifiedDoc = await Simplifier.ReduceAsync(document, Simplifier.Annotation);
            var root = await simplifiedDoc.GetSyntaxRootAsync();
            root = Formatter.Format(root, Formatter.Annotation, simplifiedDoc.Project.Solution.Workspace);
            return root.GetText().ToString();
        }
    }
}
