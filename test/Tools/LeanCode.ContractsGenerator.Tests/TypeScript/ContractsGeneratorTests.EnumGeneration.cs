using Xunit;
using static LeanCode.ContractsGenerator.Tests.TypeScript.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Order_of_enums_is_alphanumeric()
        {
            var file1 = @"
            namespace File2
            {
                public enum TestEnum3 { }
                public enum TestEnum4 { }
            }";

            var file2 = @"
            namespace File2
            {
                public enum TestEnum2 { }
                public enum TestEnum1 { }
            }";

            var generator = CreateTsGenerator(file1, file2);

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("enum TestEnum1", contracts);
            Assert.Contains("enum TestEnum2", contracts);
            Assert.Contains("enum TestEnum3", contracts);
            Assert.Contains("enum TestEnum4", contracts);

            var test1Index = contracts.IndexOf("enum TestEnum1");
            var test2Index = contracts.IndexOf("enum TestEnum2");
            var test3Index = contracts.IndexOf("enum TestEnum3");
            var test4Index = contracts.IndexOf("enum TestEnum4");

            Assert.True(test1Index < test2Index);
            Assert.True(test2Index < test3Index);
            Assert.True(test3Index < test4Index);
        }
    }
}
