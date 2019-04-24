using Xunit;
using static LeanCode.ContractsGenerator.Tests.TypeScript.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Order_of_enums_is_alphanumeric()
        {
            var generator = CreateTsGeneratorFromNamespace("public enum TestEnum2 { } public enum TestEnum1 { }");

            var contracts = GetContracts(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("enum TestEnum1", contracts);
            Assert.Contains("enum TestEnum2", contracts);

            var test1Index = contracts.IndexOf("enum TestEnum1");
            var test2Index = contracts.IndexOf("enum TestEnum2");

            Assert.True(test1Index < test2Index);
        }
    }
}
