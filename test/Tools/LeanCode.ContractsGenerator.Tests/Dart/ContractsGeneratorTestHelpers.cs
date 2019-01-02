using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using LeanCode.ContractsGenerator;
using LeanCode.ContractsGenerator.Languages;
using LeanCode.ContractsGenerator.Languages.Dart;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    static class ContractsGeneratorTestHelpers
    {
        public static LeanCode.ContractsGenerator.CodeGenerator CreateDartGeneratorFromClass(string classBody, string className = "TsGeneratorTest")
        {
            return CreateDartGeneratorFromNamespace(
            $@"
                public class {className}
                {{
                    {classBody}
                }}
            ");

        }

        public static LeanCode.ContractsGenerator.CodeGenerator CreateDartGeneratorFromNamespace(string namespaceBody, string namespaceName = "DartGenerator.Tests")
        {
            return CreateDartGenerator($@"
                using LeanCode.CQRS;
                using System.Collections.Generic;

                namespace {namespaceName}
                {{
                    {namespaceBody}
                }}
            ");
        }

        public static GeneratorConfiguration DefaultDartConfiguration = new GeneratorConfiguration()
        {
            Name = "Test",
            Dart = new DartConfiguration
            {
                ContractsPreamble = "",
            }
        };

        public static string GetContracts(IEnumerable<LanguageFileOutput> output)
        {
            return output.Where(o => o.Name.EndsWith("dart")).First().Content;
        }

        public static LeanCode.ContractsGenerator.CodeGenerator CreateDartGenerator(params string[] sources)
        {
            var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

            var compilation = CSharpCompilation.Create("DartGeneratorTests")
                .AddReferences(new[] {
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(HashSet<>).GetTypeInfo().Assembly.Location)
                })
                .AddSyntaxTrees(trees.Concat(new[] { CSharpSyntaxTree.ParseText("namespace LeanCode.CQRS { public interface IQuery<T> { } public interface ICommand { } public interface IRemoteQuery<T> : IQuery<T> { } public interface IRemoteCommand : ICommand { }}") }));

            return new LeanCode.ContractsGenerator.CodeGenerator(trees, compilation);
        }
    }
}
