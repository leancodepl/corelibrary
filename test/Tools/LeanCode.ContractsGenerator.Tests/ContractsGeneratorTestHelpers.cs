using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using LeanCode.ContractsGenerator;

namespace LeanCode.ContractsGenerator.Tests
{
    static class ContractsGeneratorTestHelpers
    {
        public static LeanCode.ContractsGenerator.CodeGenerator CreateTsGeneratorFromClass(string classBody, string className = "TsGeneratorTest")
        {
            return CreateTsGeneratorFromNamespace(
            $@"
                public class {className}
                {{
                    {classBody}
                }}
            ");

        }

        public static LeanCode.ContractsGenerator.CodeGenerator CreateTsGeneratorFromNamespace(string namespaceBody, string namespaceName = "TsGenerator.Tests")
        {
            return CreateTsGenerator($@"
                using LeanCode.CQRS;
                using System.Collections.Generic;

                namespace {namespaceName}
                {{
                    {namespaceBody}
                }}
            ");
        }

        public static LeanCode.ContractsGenerator.CodeGenerator CreateTsGenerator(params string[] sources)
        {
            var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

            var compilation = CSharpCompilation.Create("TsGeneratorTests")
                .AddReferences(new[] {
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(HashSet<>).GetTypeInfo().Assembly.Location)
                })
                .AddSyntaxTrees(trees.Concat(new[] { CSharpSyntaxTree.ParseText("namespace LeanCode.CQRS { public interface IQuery<T> { } public interface ICommand { } public interface IRemoteQuery<T> : IQuery<T> { } public interface IRemoteCommand : ICommand { }}") }));

            return new LeanCode.ContractsGenerator.CodeGenerator(trees, compilation, new GeneratorConfiguration() { ContractsPreamble = "", ClientPreamble = "", Name = "Test" });
        }
    }
}