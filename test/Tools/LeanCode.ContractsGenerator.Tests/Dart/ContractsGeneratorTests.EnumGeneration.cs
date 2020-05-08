using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Enums_have_correct_values()
        {
            var generator = CreateDartGeneratorFromNamespace("public enum TestEnum1 { New, NotNew, Default }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("enum TestEnum1", contracts);
            Assert.Contains("new_", contracts);
            Assert.Contains("default_", contracts);
            Assert.Contains("notNew", contracts);
        }

        [Fact]
        public void Enums_have_correct_json_values_1()
        {
            var generator = CreateDartGeneratorFromNamespace("public enum Cuisine { Polish, Italian, American }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Matches("@JsonValue\\(0\\)\\s*polish", contracts);
            Assert.Matches("@JsonValue\\(1\\)\\s*italian", contracts);
            Assert.Matches("@JsonValue\\(2\\)\\s*american", contracts);
        }

        [Fact]
        public void Enums_have_correct_json_values_2()
        {
            var generator = CreateDartGeneratorFromNamespace("public enum Cuisine { Polish = 2, Italian = 3, American = 4 }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Matches("@JsonValue\\(2\\)\\s*polish", contracts);
            Assert.Matches("@JsonValue\\(3\\)\\s*italian", contracts);
            Assert.Matches("@JsonValue\\(4\\)\\s*american", contracts);
        }
    }
}
