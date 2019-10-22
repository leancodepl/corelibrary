using LeanCode.ContractsGenerator.Languages.TypeScript;
using Xunit;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Empty_configuration_contains_default_preambles()
        {
            var configuration = GeneratorConfiguration.DeserializeConfigurationsFromString(@"
                [
                    {
                        ""TypeScript"": { }
                    }
                ]")[0].TypeScript;

            var defaultConfiguration = new TypeScriptConfiguration();

            Assert.Equal(
                defaultConfiguration.ContractsPreamble,
                configuration.ContractsPreamble);
        }

        [Fact]
        public void Configuration_with_preamble_lines_is_correctly_interpreted()
        {
            var configuration = GeneratorConfiguration.DeserializeConfigurationsFromString(@"
                [
                    {
                        ""TypeScript"": {
                            ""ContractsPreambleLines"": [
                                ""ContractsFirstLine"",
                                ""ContractsSecondLine"",
                                ""ContractsThirdLine"",
                                """"
                            ]
                        }
                    }
                ]")[0].TypeScript;

            var expectedContractsPreamble = "ContractsFirstLine\nContractsSecondLine\nContractsThirdLine\n";

            Assert.Equal(expectedContractsPreamble, configuration.ContractsPreamble);
        }

        [Fact]
        public void Configuration_with_joined_preamble_is_correctly_interpreted()
        {
            var configuration = GeneratorConfiguration.DeserializeConfigurationsFromString(@"
                [
                    {
                        ""TypeScript"": {
                            ""ContractsPreamble"": ""ContractsFirstLine\nContractsSecondLine\nContractsThirdLine\n""
                        }
                    }
                ]")[0].TypeScript;

            var expectedContractsPreamble = "ContractsFirstLine\nContractsSecondLine\nContractsThirdLine\n";

            Assert.Equal(expectedContractsPreamble, configuration.ContractsPreamble);
        }
    }
}
