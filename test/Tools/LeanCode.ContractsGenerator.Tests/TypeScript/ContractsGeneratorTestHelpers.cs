using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using LeanCode.ContractsGenerator.Languages.TypeScript;
using LeanCode.ContractsGenerator.Languages;
using LeanCode.CQRS;
using System.Reflection;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    static class ContractsGeneratorTestHelpers
    {
        public static CodeGenerator CreateTsGeneratorFromClass(string classBody, string className = "TsGeneratorTest")
        {
            return CreateTsGeneratorFromNamespace(
            $@"
                public class {className}
                {{
                    {classBody}
                }}
            ");

        }

        public static CodeGenerator CreateTsGeneratorFromNamespace(string namespaceBody, string namespaceName = "TsGenerator.Tests")
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

        public static CodeGenerator CreateTsGenerator(params string[] sources)
        {
            var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

            var compilation = CSharpCompilation.Create("TsGeneratorTests")
                .AddReferences(new[] {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(HashSet<>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location),
                })
                .AddSyntaxTrees(trees);

            return new CodeGenerator(trees, compilation);
        }
    }
}
