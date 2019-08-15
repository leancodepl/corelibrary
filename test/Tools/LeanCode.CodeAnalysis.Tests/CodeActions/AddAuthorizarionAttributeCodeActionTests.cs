using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.CodeActions;
using LeanCode.CQRS;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.CodeActions
{
    public class AddAuthorizarionAttributeCodeActionTests : IDisposable
    {
        private readonly AdhocWorkspace workspace;
        private readonly Project project;

        public AddAuthorizarionAttributeCodeActionTests()
        {
            workspace = new AdhocWorkspace();
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, "NewProject", "projName", LanguageNames.CSharp);
            project = workspace.AddProject(projectInfo)
                .AddMetadataReferences(new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                });
        }

        [Fact]
        public async Task Adds_attribute()
        {
            var source =
@"
using LeanCode.CQRS;
using LeanCode.CQRS.Security;

public class Cmd : ICommand
{}";

            var expected =
            @"
using LeanCode.CQRS;
using LeanCode.CQRS.Security;

[AllowUnauthorized]
public class Cmd : ICommand
{}";

            var actual = await ApplyFix(source);

            Assert.Equal(expected, actual);
        }

        private async Task<string> ApplyFix(string code)
        {
            var doc = AddDocument(code);
            var root = await doc.GetSyntaxRootAsync();
            var decl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var action = new AddAuthorizationAttributeCodeAction(
                doc,
                decl.Span,
                "AllowUnauthorized",
                "LeanCode.CQRS.Security");

            var modified = await action.Test();
            var text = await modified.GetTextAsync();
            return text.ToString();
        }

        private Document AddDocument(string code)
        {
            var docName = "A" + Guid.NewGuid().ToString("N") + ".cs";
            var sourceText = SourceText.From(code);
            return workspace.AddDocument(project.Id, docName, sourceText);
        }

        public void Dispose()
        {
            workspace.Dispose();
        }
    }
}
