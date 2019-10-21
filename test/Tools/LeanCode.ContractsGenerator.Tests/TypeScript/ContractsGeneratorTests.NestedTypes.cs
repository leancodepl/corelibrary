using Xunit;
using static LeanCode.ContractsGenerator.Tests.TypeScript.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Generates_globals_if_type_has_consts()
        {
            var generator = CreateTsGeneratorFromNamespace(
                @"
                public class Parent
                {
                    public const int Const = 2;
                }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Parent", client);
            Assert.Contains("Const", client);
        }

        [Fact]
        public void Generates_globals_if_type_children_have_consts()
        {
            var generator = CreateTsGeneratorFromNamespace(
                @"
                public class Parent
                {
                    public class Child
                    {
                        public const int Const = 2;
                    }
                }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Parent", client);
            Assert.Contains("Child", client);
            Assert.Contains("Const", client);
        }

        [Fact]
        public void Does_prepend_name_of_the_parent()
        {
            var generator = CreateTsGeneratorFromNamespace(
                @"
                public class Parent
                {
                    public class Child
                    { }
                }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Parent_Child", client);
        }
    }
}
