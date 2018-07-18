using Xunit;
using static LeanCode.ContractsGenerator.Tests.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Namespace_name_with_one_element_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass : IRemoteCommand { }", "TsGenerator");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("testClass: cqrsClient\\.executeCommand\\.bind\\(cqrsClient, \"TsGenerator\\.TestClass\"\\)", client);
        }

        [Fact]
        public void Namespace_name_with_multiple_elements_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass : IRemoteCommand { }", "TsGenerator.TestNamespace1.TestNamespace2");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("testClass: cqrsClient\\.executeCommand\\.bind\\(cqrsClient, \"TsGenerator\\.TestNamespace1\\.TestNamespace2\\.TestClass\"\\)", client);
        }
    }
}
