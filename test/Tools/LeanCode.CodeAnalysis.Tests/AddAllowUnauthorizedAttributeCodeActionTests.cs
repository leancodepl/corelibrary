using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.CodeActions;
using LeanCode.CQRS;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests
{
    public class AddAllowUnauthorizedAttributeCodeActionTests
    {
        [Fact]
        public async Task AddsAttribute()
        {
            var doc = LoadDocument(@"
            using LeanCode.CQRS;
            public class Command : ICommand
            {}
            ");

            var root = await doc.GetSyntaxRootAsync();
            var decl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var action = new AddAllowUnauthorizedAttributeCodeAction(doc, decl.Span);

            var modified = await action.Test();
            root = await modified.GetSyntaxRootAsync();
        }

        private static Document LoadDocument(string code)
        {
            var workspace = new AdhocWorkspace();
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, "NewProject", "projName", LanguageNames.CSharp);
            var newProject = workspace.AddProject(projectInfo)
                .AddMetadataReferences(new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                });

            var sourceText = SourceText.From(code);
            // sourceText.
            return workspace.AddDocument(newProject.Id, "NewFile.cs", sourceText);
        }
    }
}
