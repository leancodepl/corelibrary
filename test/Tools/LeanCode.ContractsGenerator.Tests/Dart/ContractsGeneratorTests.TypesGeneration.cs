using LeanCode.ContractsGenerator.Languages.Dart;
using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public class ContractsGeneratorTests_TypesGeneration
    {
        [Fact]
        public void Imports_are_generated()
        {
            var generator = CreateDartGeneratorFromClass("");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("import 'package:json_annotation/json_annotation.dart';", contracts);
            Assert.Contains("import 'package:cqrs/cqrs.dart';", contracts);
        }

        [Fact]
        public void Part_uses_name_from_generator_configuration_for_default_preamble()
        {
            var generator = CreateDartGeneratorFromClass("");
            var cfg = new GeneratorConfiguration()
            {
                Name = "Name",
                Dart = new DartConfiguration
                {
                },
            };

            var contracts = GetContracts(generator.Generate(cfg));

            Assert.Contains("part 'Name.g.dart';", contracts);
        }

        [Fact]
        public void List_helper_is_generated()
        {
            var generator = CreateDartGeneratorFromClass("");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("List<T> _listFromJson<T>(", contracts);
        }

        [Fact]
        public void DateTime_helpers_is_generated()
        {
            var generator = CreateDartGeneratorFromClass("");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("DateTime _dateTimeFromJson(String value)", contracts);
            Assert.Contains("DateTime _nullableDateTimeFromJson(String value)", contracts);
        }

        [Fact]
        public void Double_helpers_is_generated()
        {
            var generator = CreateDartGeneratorFromClass("");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("double _doubleFromJson(dynamic value)", contracts);
            Assert.Contains("double _nullableDoubleFromJson(dynamic value)", contracts);
        }

        [Fact]
        public void Int_is_resolved_to_int()
        {
            var generator = CreateDartGeneratorFromClass("public int TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("int testVar;", contracts);
        }

        [Fact]
        public void Double_uses_custom_from_json_method()
        {
            var generator = CreateDartGeneratorFromClass("public double Field { get; set; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("@JsonKey(name: 'Field', fromJson: _doubleFromJson)", contracts);
            Assert.Contains("double field;", contracts);
        }

        [Fact]
        public void Nullable_double_uses_custom_from_json_method()
        {
            var generator = CreateDartGeneratorFromClass("public double? Field { get; set; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("@JsonKey(name: 'Field', nullable: true, fromJson: _nullableDoubleFromJson)", contracts);
            Assert.Contains("double field;", contracts);
        }

        [Fact]
        public void Float_uses_custom_from_json_method()
        {
            var generator = CreateDartGeneratorFromClass("public float Field { get; set; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("@JsonKey(name: 'Field', fromJson: _doubleFromJson)", contracts);
            Assert.Contains("double field;", contracts);
        }

        [Fact]
        public void Nullable_float_uses_custom_from_json_method()
        {
            var generator = CreateDartGeneratorFromClass("public float? Field { get; set; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("@JsonKey(name: 'Field', nullable: true, fromJson: _nullableDoubleFromJson)", contracts);
            Assert.Contains("double field;", contracts);
        }

        [Fact]
        public void Decimal_uses_custom_from_json_method()
        {
            var generator = CreateDartGeneratorFromClass("public decimal Field { get; set; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("@JsonKey(name: 'Field', fromJson: _doubleFromJson)", contracts);
            Assert.Contains("double field;", contracts);
        }

        [Fact]
        public void Nullable_decimal_uses_custom_from_json_method()
        {
            var generator = CreateDartGeneratorFromClass("public decimal? Field { get; set; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("@JsonKey(name: 'Field', nullable: true, fromJson: _nullableDoubleFromJson)", contracts);
            Assert.Contains("double field;", contracts);
        }

        [Fact]
        public void DateTime_uses_custom_from_json_method()
        {
            var generator = CreateDartGeneratorFromClass("public DateTime TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("@JsonKey(name: 'TestVar', fromJson: _dateTimeFromJson)", contracts);
            Assert.Contains("DateTime testVar;", contracts);
        }

        [Fact]
        public void Nullable_DateTime_uses_custom_from_json_method()
        {
            var generator = CreateDartGeneratorFromClass("public DateTime? TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("@JsonKey(name: 'TestVar', nullable: true, fromJson: _nullableDateTimeFromJson)", contracts);
            Assert.Contains("DateTime testVar;", contracts);
        }

        [Fact]
        public void Nullable_property_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromClass("public Nullable<int> TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("name: 'TestVar', nullable: true", contracts);
            Assert.Contains("int testVar;", contracts);
        }

        [Fact]
        public void Nullable_property_with_question_mark_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromClass("public int? TestVar { get; set; };");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("name: 'TestVar', nullable: true", contracts);
            Assert.Contains("int testVar;", contracts);
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
