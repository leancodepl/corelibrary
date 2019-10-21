using Xunit;
using static LeanCode.ContractsGenerator.Tests.TypeScript.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Namespace_name_with_one_element_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass : IRemoteCommand { }", "TsGenerator");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("testClass: (dto: TestClass) => cqrsClient.executeCommand<{}>(\"TsGenerator.TestClass\", dto)", client);
        }

        [Fact]
        public void Namespace_name_with_multiple_elements_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass : IRemoteCommand { }", "TsGenerator.TestNamespace1.TestNamespace2");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("testClass: (dto: TestClass) => cqrsClient.executeCommand<{}>(\"TsGenerator.TestNamespace1.TestNamespace2.TestClass\", dto)", client);
        }
    }
}
