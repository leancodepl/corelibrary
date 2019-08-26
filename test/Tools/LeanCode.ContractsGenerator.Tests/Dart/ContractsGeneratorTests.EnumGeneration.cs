using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Order_of_enums_is_alphanumeric()
        {
            var generator = CreateDartGeneratorFromNamespace("public enum TestEnum2 { } public enum TestEnum1 { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class TestEnum1", contracts);
            Assert.Contains("class TestEnum2", contracts);

            var test1Index = contracts.IndexOf("class TestEnum1");
            var test2Index = contracts.IndexOf("class TestEnum2");

            Assert.True(test1Index < test2Index);
        }

        [Fact]
        public void Equality_operator_is_generated_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public enum TestEnum1 { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            var firstLine = "bool operator ==(Object other) =>";
            var secondLine = "other is TestEnum1 && value == other.value;";

            Assert.Contains(firstLine, contracts);
            Assert.Contains(secondLine, contracts);

            var operatorFirstLine = contracts.IndexOf(firstLine);
            var operatorSecondLine = contracts.IndexOf(secondLine);

            Assert.True(operatorFirstLine < operatorSecondLine);
        }

        [Fact]
        public void HashCode_property_is_generated_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public enum TestEnum1 { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("int get hashCode => value.hashCode;", contracts);
        }
    }
}
