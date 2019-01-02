using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using LeanCode.ContractsGenerator;
using LeanCode.ContractsGenerator.Languages.TypeScript;
using LeanCode.ContractsGenerator.Languages;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
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

        public static GeneratorConfiguration DefaultTypeScriptConfiguration = new GeneratorConfiguration()
        {
            Name = "Test",
            TypeScript = new TypeScriptConfiguration
            {
                ContractsPreamble = "",
                ClientPreamble = ""
            }
        };

        public static string GetContracts(IEnumerable<LanguageFileOutput> output)
        {
            return output.Where(o => o.Name.EndsWith("d.ts")).First().Content;
        }

        public static string GetClient(IEnumerable<LanguageFileOutput> output)
        {
            return output.Where(o => o.Name.EndsWith("Client.ts")).First().Content;
        }

        public static LeanCode.ContractsGenerator.CodeGenerator CreateTsGenerator(params string[] sources)
        {
            var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

            var compilation = CSharpCompilation.Create("TsGeneratorTests")
                .AddReferences(new[] {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(HashSet<>).Assembly.Location)
                })
                .AddSyntaxTrees(trees.Concat(new[] { CSharpSyntaxTree.ParseText("namespace LeanCode.CQRS { public interface IQuery<T> { } public interface ICommand { } public interface IRemoteQuery<T> : IQuery<T> { } public interface IRemoteCommand : ICommand { }}") }));

            return new LeanCode.ContractsGenerator.CodeGenerator(trees, compilation);
        }
    }
}
