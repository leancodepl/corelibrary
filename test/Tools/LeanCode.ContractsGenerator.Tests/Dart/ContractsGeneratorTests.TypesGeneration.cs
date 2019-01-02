using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Int_is_resolved_to_int()
        {
            var generator = CreateDartGeneratorFromClass("public int TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("int testVar;", contracts);
        }

        [Fact]
        public void Nullable_property_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromClass("public Nullable<int> TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("int testVar", contracts);
        }

        [Fact]
        public void Nullable_property_with_question_mark_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromClass("public int? TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("int testVar", contracts);
        }

        [Fact]
        public void Array_is_resolved_to_list()
        {
            var generator = CreateDartGeneratorFromClass("public int[] TestArray { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("List<int> testArray", contracts);
        }

        [Fact]
        public void List_is_resolved_to_list()
        {
            var generator = CreateDartGeneratorFromClass("public List<int> TestArray { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("List<int> testArray", contracts);
        }

        [Fact]
        public void HashSet_is_resolved_to_list()
        {
            var generator = CreateDartGeneratorFromClass("public HashSet<int> TestArray { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("List<int> testArray", contracts);
        }

        [Fact]
        public void Dictionary_is_resolved_to_map()
        {
            var generator = CreateDartGeneratorFromClass("public Dictionary<string, int> TestDictionary { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("Map<String, int> testDictionary", contracts);
        }
    }
}
