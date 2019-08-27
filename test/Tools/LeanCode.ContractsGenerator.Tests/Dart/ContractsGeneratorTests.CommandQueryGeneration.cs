using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Remote_query_contains_mapping_from_json_for_DTO()
        {
            var generator = CreateDartGeneratorFromNamespace("public class Query : IRemoteQuery<DTO> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("DTO resultFactory(dynamic decodedJson) => DTO.fromJson(decodedJson);", contracts);
        }

        [Fact]
        public void Remote_query_contains_cast_to_type_for_string()
        {
            var generator = CreateDartGeneratorFromNamespace("public class Query : IRemoteQuery<string> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("String resultFactory(dynamic decodedJson) => decodedJson as String;", contracts);
        }

        [Fact]
        public void Remote_query_contains_cast_to_type_for_int()
        {
            var generator = CreateDartGeneratorFromNamespace("public class Query : IRemoteQuery<int> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("int resultFactory(dynamic decodedJson) => decodedJson as int;", contracts);
        }

        [Fact]
        public void Remote_query_contains_cast_to_type_for_bool()
        {
            var generator = CreateDartGeneratorFromNamespace("public class Query : IRemoteQuery<bool> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("bool resultFactory(dynamic decodedJson) => decodedJson as bool;", contracts);
        }

        [Fact]
        public void Remote_query_contains_cast_to_type_for_datetime()
        {
            var generator = CreateDartGeneratorFromNamespace("public class Query : IRemoteQuery<DateTime> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("DateTime resultFactory(dynamic decodedJson) => decodedJson as DateTime;", contracts);
        }

        [Fact]
        public void Remote_query_contains_cast_to_type_for_guid()
        {
            var generator = CreateDartGeneratorFromNamespace("public class Query : IRemoteQuery<Guid> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("String resultFactory(dynamic decodedJson) => decodedJson as String;", contracts);
        }

        [Fact]
        public void Remote_query_contains_cast_to_type_for_double()
        {
            var generator = CreateDartGeneratorFromNamespace("public class Query : IRemoteQuery<double> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("double resultFactory(dynamic decodedJson) => decodedJson as double;", contracts);
        }

        [Fact]
        public void Remote_query_contains_cast_to_type_for_decimal()
        {
            var generator = CreateDartGeneratorFromNamespace("public class Query : IRemoteQuery<decimal> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("double resultFactory(dynamic decodedJson) => decodedJson as double;", contracts);
        }

        [Fact]
        public void Remote_query_contains_cast_to_type_for_dynamic()
        {
            var generator = CreateDartGeneratorFromNamespace("public class Query : IRemoteQuery<dynamic> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("dynamic resultFactory(dynamic decodedJson) => decodedJson as dynamic;", contracts);
        }

        [Fact]
        public void Remote_query_contains_cast_to_type_for_object()
        {
            var generator = CreateDartGeneratorFromNamespace("public class Query : IRemoteQuery<object> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("Object resultFactory(dynamic decodedJson) => decodedJson as Object;", contracts);
        }
    }
}
