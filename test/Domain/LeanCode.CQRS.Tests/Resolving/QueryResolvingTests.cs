using System.Threading.Tasks;
using Xunit;

namespace LeanCode.CQRS.Tests
{
    public class QueryResolvingTests : BaseCQRSTests
    {
        public QueryResolvingTests()
        {
            Prepare();
        }

        [Fact]
        public void Resolves_QH_for_existing_query()
        {
            var (handler, underlying) = FindSampleQueryHandler();

            Assert.NotNull(handler);
            Assert.NotNull(underlying);
        }

        [Fact]
        public void Resolves_diffrent_QH_if_called_multiple_times_for_existing_query()
        {
            var (handler1, _) = FindSampleQueryHandler();
            var (handler2, _) = FindSampleQueryHandler();

            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.NotSame(handler1, handler2);
        }

        [Fact]
        public void Resolves_different_instances_of_underlying_QH_when_resolving_the_same_query_multiple_times()
        {
            var (_, underlying1) = FindSampleQueryHandler();
            var (_, underlying2) = FindSampleQueryHandler();

            Assert.NotNull(underlying1);
            Assert.NotNull(underlying2);
            Assert.NotSame(underlying1, underlying2);
        }

        [Fact]
        public void The_wrapper_type_is_the_same_when_resolved_the_same_query_multiple_times()
        {
            var (handler1, _) = FindSampleQueryHandler();
            var (handler2, _) = FindSampleQueryHandler();

            Assert.Same(handler1.GetType(), handler2.GetType());
        }

        [Fact]
        public void Resolves_null_if_QH_is_not_found()
        {
            var handler = QHResolver.FindQueryHandler(typeof(NoQHQuery));

            Assert.Null(handler);
        }

        [Fact]
        public async Task The_data_is_correctly_passed_to_underlying_QH()
        {
            var objCtx = new ObjContext();
            var query = new SampleQuery();
            var resultObj = new object();

            var (handler, underlying) = FindSampleQueryHandler();
            underlying.Result = resultObj;

            var result = await handler.ExecuteAsync(objCtx, query);

            Assert.Equal(query, underlying.Query);
            Assert.Equal(objCtx, underlying.Context);
            Assert.Equal(resultObj, result);
        }
    }
}
