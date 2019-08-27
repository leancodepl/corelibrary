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
            Assert.Contains("'TestVar': testVar.toIso8601String(),", contracts);
            Assert.Contains("..testVar = map['TestVar'] != null ? DateTime.parse(normalizeDate(map['TestVar'])) : null;", contracts);
        }

        [Fact]
        public void Nullable_DateTime_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromClass("public DateTime? TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("DateTime testVar;", contracts);
            Assert.Contains("'TestVar': testVar?.toIso8601String(),", contracts);
            Assert.Contains("..testVar = map['TestVar'] != null ? DateTime.parse(normalizeDate(map['TestVar'])) : null;", contracts);
        }

        [Fact]
        public void Nullable_property_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromClass("public Nullable<int> TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Matches("@nullable\n\\s*int testVar", contracts);
        }

        [Fact]
        public void Nullable_property_with_question_mark_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromClass("public int? TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Matches("@nullable\n\\s*int testVar", contracts);
        }

        [Fact]
        public void Nullable_property_does_not_produce_attribute_inside_to_json()
        {
            var generator = CreateDartGeneratorFromNamespace(
@"public class SomeDTO
{
    public CurrencyDTO? Currency { get; set; }
}

public enum CurrencyDTO
{
    PLN = 0,
    EUR = 1,
}");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Matches("@nullable\n\\s*CurrencyDTO currency", contracts);
            Assert.Contains("..currency = map['Currency'] != null ? CurrencyDTO.fromJson(map['Currency']) : null;", contracts);
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

        [Fact]
        public void List_containing_list_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromClass("public List<List<int>> TestListOfLists { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("List<List<int>> testListOfLists", contracts);
        }

        [Fact]
        public void List_containing_custom_type_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass { public List<DTO> CustomElementList { get; set; } }\npublic class DTO { public int Field { get; set; } } ");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("List<DTO> customElementList", contracts);
        }
    }
}
