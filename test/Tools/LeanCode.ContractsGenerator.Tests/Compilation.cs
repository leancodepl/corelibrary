using System.Collections.Generic;
using System.Linq;
using LeanCode.CQRS;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LeanCode.ContractsGenerator.Tests
{
    public static class Compilation
    {
        private static readonly AssemblyMetadata[] References;

        static Compilation()
        {
            References = new[]
            {
                AssemblyMetadata.CreateFromFile(typeof(object).Assembly.Location),
                AssemblyMetadata.CreateFromFile(typeof(HashSet<>).Assembly.Location),
                AssemblyMetadata.CreateFromFile(typeof(ICommand).Assembly.Location),
            };
        }

        public static (List<SyntaxTree>, CSharpCompilation) Create(params string[] sources)
        {
            var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

            var compilation = CSharpCompilation.Create("TsGeneratorTests")
                .AddReferences(References.Select(r => r.GetReference()))
                .AddSyntaxTrees(trees);

            return (trees, compilation);
        }
    }
}
