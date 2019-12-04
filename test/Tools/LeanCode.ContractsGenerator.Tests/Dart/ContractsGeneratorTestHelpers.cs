using System.Collections.Generic;
using System.Linq;
using LeanCode.ContractsGenerator.Languages;
using LeanCode.ContractsGenerator.Languages.Dart;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    internal static class ContractsGeneratorTestHelpers
    {
        public static CodeGenerator CreateDartGeneratorFromClass(string classBody, string className = "DartGeneratorTest")
        {
            return CreateDartGeneratorFromNamespace(
            $@"
                public class {className}
                {{
                    {classBody}
                }}
            ");
        }

        public static CodeGenerator CreateDartGeneratorFromNamespace(string namespaceBody, string namespaceName = "DartGenerator.Tests")
        {
            return CreateDartGenerator($@"
                using System;
                using LeanCode.CQRS;
                using System.Collections.Generic;

                namespace {namespaceName}
                {{
                    {namespaceBody}
                }}
            ");
        }

        public static readonly GeneratorConfiguration DefaultDartConfiguration = new GeneratorConfiguration()
        {
            Name = "Test",
            Dart = new DartConfiguration { },
        };

        public static string GetContracts(IEnumerable<LanguageFileOutput> output)
        {
            return output.Where(o => o.Name.EndsWith("dart")).First().Content;
        }

        public static CodeGenerator CreateDartGenerator(params string[] sources)
        {
            var (trees, compilation) = Compilation.Create(sources);
            return new CodeGenerator(trees, compilation);
        }
    }
}
