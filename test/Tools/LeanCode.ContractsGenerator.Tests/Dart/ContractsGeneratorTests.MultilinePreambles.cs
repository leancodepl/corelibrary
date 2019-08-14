using LeanCode.ContractsGenerator.Languages.Dart;
using Xunit;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Empty_configuration_contains_default_preamble()
        {
            var configuration = GeneratorConfiguration.DeserializeConfigurationsFromString(@"
                [
                    {
                        Dart: { }
                    }
                ]")[0].Dart;

            var defaultConfiguration = new DartConfiguration();

            Assert.Equal(
                defaultConfiguration.ContractsPreambleLines,
                configuration.ContractsPreambleLines);

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
                        Dart: {
                            ContractsPreambleLines: [
                                ""ContractsFirstLine"",
                                ""ContractsSecondLine"",
                                ""ContractsThirdLine"",
                                """"
                            ]
                        }
                    }
                ]")[0].Dart;

            var expectedContractsPreamble = "ContractsFirstLine\nContractsSecondLine\nContractsThirdLine\n";

            Assert.Equal(expectedContractsPreamble, configuration.ContractsPreamble);
        }

        [Fact]
        public void Configuration_with_joined_preamble_is_correctly_interpreted()
        {
            var configuration = GeneratorConfiguration.DeserializeConfigurationsFromString(@"
                [
                    {
                        Dart: {
                            ContractsPreamble: ""ContractsFirstLine\nContractsSecondLine\nContractsThirdLine\n""
                        }
                    }
                ]")[0].Dart;

            var expectedContractsPreamble = "ContractsFirstLine\nContractsSecondLine\nContractsThirdLine\n";

            Assert.Equal(expectedContractsPreamble, configuration.ContractsPreamble);
        }
    }
}
