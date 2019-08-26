using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Nullable_annotation_is_generated()
        {
            var generator = CreateDartGeneratorFromClass(string.Empty);

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class _Nullable { const _Nullable(); } const nullable = _Nullable();", contracts);
        }

        [Fact]
        public void Int_is_resolved_to_int()
        {
            var generator = CreateDartGeneratorFromClass("public int TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("int testVar;", contracts);
        }

        [Fact]
        public void Double_is_resolved_as_string_for_nan_or_infinity()
        {
            var generator = CreateDartGeneratorFromClass("public double Field { get; set; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("..field = map['Field'] is String ?\ndouble.parse(map['Field']) : map['Field']", contracts);
        }

        [Fact]
        public void DateTime_is_resolved_to_DateTime()
        {
            var generator = CreateDartGeneratorFromClass("public DateTime TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("DateTime testVar;", contracts);
        }

        [Fact]
        public void Nullable_property_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromClass("public Nullable<int> TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("@nullable\nint testVar", contracts);
        }

        [Fact]
        public void Nullable_property_with_question_mark_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromClass("public int? TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("@nullable\nint testVar", contracts);
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
