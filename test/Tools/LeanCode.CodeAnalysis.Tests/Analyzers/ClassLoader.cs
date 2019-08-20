using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LeanCode.CQRS;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LeanCode.CodeAnalysis.Tests.Analyzers
{
    public static class ClassLoader
    {
        public static IEnumerable<object[]> LoadTestContracts(string file)
        {
            var content = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(content);

            var compilation = CSharpCompilation.Create(
                "LeanCode.CodeAnalysis.Tests",
                new[] { tree },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                });

            var model = compilation.GetSemanticModel(tree);

            var classes = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

            return classes.Select(c => new object[]
            {
                compilation, model.GetDeclaredSymbol(c),
            });
        }
    }
}
