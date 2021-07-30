using System.Collections.Generic;
using System.Linq;
using LeanCode.ContractsGenerator.Languages;
using LeanCode.ContractsGenerator.Languages.TypeScript;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    internal static class ContractsGeneratorTestHelpers
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

        public static readonly GeneratorConfiguration DefaultTypeScriptConfiguration = new()
        {
            Name = "Test",
            TypeScript = new TypeScriptConfiguration
            {
                ContractsPreamble = "",
            },
        };

        public static string GetClient(IEnumerable<LanguageFileOutput> output)
        {
            return output.Where(o => o.Name.EndsWith("Client.ts")).First().Content;
        }

        public static CodeGenerator CreateTsGenerator(params string[] sources)
        {
            var (trees, compilation) = Compilation.Create(sources);
            return new CodeGenerator(trees, compilation);
        }
    }
}
