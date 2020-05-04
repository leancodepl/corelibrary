using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Remote_query_contains_mapping_from_json_for_DTO()
        {
            var generator = CreateDartGeneratorFromNamespace("public class DTO {} public class Query : IRemoteQuery<DTO> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("DTO resultFactory(dynamic decodedJson) => _$DTOFromJson(decodedJson as Map<String, dynamic>);", contracts);
        }

        [Fact]
        public void Remote_query_contains_mapping_from_json_for_list()
        {
            var generator = CreateDartGeneratorFromNamespace("public class DTO {} public class Query : IRemoteQuery<List<DTO>> { }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("List<DTO> resultFactory(dynamic decodedJson) => _listFromJson(decodedJson as Iterable<dynamic>, _$DTOFromJson);", contracts);
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
        public void Remote_query_has_base_class_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace(
@"public class BaseClass
{
    public int Field { get; set; }
}

[AuthorizeWhenHasAnyOf(KnownRoles.Admin)]
public class FirestoreAdminToken : BaseClass, IRemoteQuery<string>
{ }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class FirestoreAdminToken extends BaseClass implements IRemoteQuery<String>", contracts);
        }

        [Fact]
        public void Remote_query_has_ignored_interface()
        {
            var generator = CreateDartGeneratorFromNamespace(
@"[AuthorizeWhenHasAnyOf(KnownRoles.Admin)]
public class GetOfferData : IRemoteQuery<string>, IMyOffer
{ }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("class GetOfferData implements IRemoteQuery<String>", contracts);
            Assert.DoesNotContain("IMyOffer", contracts);
        }
    }
}
