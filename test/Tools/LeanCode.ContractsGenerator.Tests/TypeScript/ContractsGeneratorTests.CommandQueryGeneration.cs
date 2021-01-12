using Xunit;
using static LeanCode.ContractsGenerator.Tests.TypeScript.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public class ContractsGeneratorTests_CommandQueryGeneration
    {
        [Fact]
        public void Namespace_name_with_one_element_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass : IRemoteCommand { }", "TsGenerator");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("testClass: (dto: TestClass) => cqrsClient.executeCommand<{}>(\"TsGenerator.TestClass\", dto)", client);
        }

        [Fact]
        public void Namespace_name_with_multiple_elements_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass : IRemoteCommand { }", "TsGenerator.TestNamespace1.TestNamespace2");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("testClass: (dto: TestClass) => cqrsClient.executeCommand<{}>(\"TsGenerator.TestNamespace1.TestNamespace2.TestClass\", dto)", client);
        }

        [Fact]
        public void Complex_abstract_query_tree_gets_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace(
                @"
                public class TransactionItemDTO { }

                public abstract class PaginatedTransactionItemsDTOBase<T>
                    where T : TransactionItemDTO
                {
                    public int Count { get; set; }
                    public List<T> Items { get; set; }
                }

                public class PaginatedTransactionItemsDTO
                    : PaginatedTransactionItemsDTOBase<TransactionItemDTO>
                { }

                public abstract class GetTransactionItemsBase<TDto, TPaginatedCollection>
                    : IRemoteQuery<TPaginatedCollection>
                    where TDto : TransactionItemDTO
                    where TPaginatedCollection : PaginatedTransactionItemsDTOBase<TDto>
                {
                    public DateTime? InclusivePaymentDateFrom { get; set; }
                }

                public class GetTransactionItems
                    : GetTransactionItemsBase<TransactionItemDTO, PaginatedTransactionItemsDTO>
                { }
            ", "TsGenerator");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("getTransactionItems: (dto: GetTransactionItems) => cqrsClient.executeQuery<PaginatedTransactionItemsDTO>(\"TsGenerator.GetTransactionItems\", dto)", client);
        }
    }
}
