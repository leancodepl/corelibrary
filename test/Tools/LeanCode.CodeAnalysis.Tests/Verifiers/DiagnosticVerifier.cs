using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Verifiers
{
    public abstract class DiagnosticVerifier : IDisposable
    {
        protected abstract DiagnosticAnalyzer GetDiagnosticAnalyzer();
        protected AdhocWorkspace Workspace { get; } = new AdhocWorkspace();

        protected Task VerifyDiagnostics(string source, params DiagnosticResult[] expected)
        {
            return VerifyDiagnostics(new[] { source }, expected);
        }

        protected async Task VerifyDiagnostics(string[] sources, params DiagnosticResult[] expected)
        {
            var analyzer = GetDiagnosticAnalyzer();
            var diagnostics = await GetSortedDiagnostics(sources, analyzer);
            var actual = diagnostics.Select(d => new DiagnosticResult(d));

            Assert.Equal(expected, actual);
        }

        private static readonly MetadataReference[] CommonReferences = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommandHandler<,>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ContextualValidator<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(AbstractValidator<>).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
        };

        private Task<List<Diagnostic>> GetSortedDiagnostics(string[] sources, DiagnosticAnalyzer analyzer)
        {
            return GetSortedDiagnosticsFromDocuments(analyzer, GetDocuments(sources));
        }

        protected static async Task<List<Diagnostic>> GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer analyzer, Document[] documents)
        {
            var projects = documents.Select(d => d.Project).ToHashSet();
            var trees = (await Task.WhenAll(documents.Select(d => d.GetSyntaxTreeAsync()))).ToHashSet();

            var diagnostics = new List<Diagnostic>();
            foreach (var project in projects)
            {
                var compilation = await project.GetCompilationAsync();
                var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
                var diags = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
                foreach (var diag in diags)
                {
                    if (diag.Location == Location.None || diag.Location.IsInMetadata)
                    {
                        diagnostics.Add(diag);
                    }
                    else if (trees.Contains(diag.Location.SourceTree))
                    {
                        // Add diagnostics only for current project
                        diagnostics.Add(diag);
                    }
                }
            }

            diagnostics.Sort((a, b) => a.Location.SourceSpan.Start.CompareTo(b.Location.SourceSpan.Start));
            return diagnostics;
        }

        private Document[] GetDocuments(string[] sources)
        {
            var project = CreateProject(sources);
            var documents = project.Documents.ToArray();

            if (sources.Length != documents.Length)
            {
                throw new InvalidOperationException("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }

        protected Document CreateDocument(string source)
        {
            return CreateProject(new[] { source }).Documents.First();
        }

        private Project CreateProject(string[] sources)
        {
            const string TestProjectName = "TestProjectName";

            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

            var solution = Workspace
                .CurrentSolution
                .AddProject(projectId, TestProjectName, TestProjectName, LanguageNames.CSharp)
                .AddMetadataReferences(projectId, CommonReferences)
                .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            int count = 0;
            foreach (var source in sources)
            {
                var newFileName = "Test" + count + "." + "cs";
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }

            return solution.GetProject(projectId);
        }

        public void Dispose()
        {
            Workspace.Dispose();
        }
    }
}
