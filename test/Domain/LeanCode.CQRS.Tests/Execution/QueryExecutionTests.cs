using System.Threading.Tasks;
using Autofac;
using LeanCode.CQRS.Execution;
using Xunit;

namespace LeanCode.CQRS.Tests
{
    public class QueryExecutionTests : BaseCQRSTests
    {
        [Fact]
        public async Task Executes_query_handler_with_correct_data()
        {
            Prepare();

            var appCtx = new AppContext();
            var objCtx = new ObjContext();
            var query = new SingleInstanceQuery();
            var expResult = new object();
            var handler = Container.Resolve<SingleInstanceQueryHandler>();
            handler.Result = expResult;

            var result = await QueryExecutor.GetAsync(appCtx, objCtx, query);

            Assert.Equal(expResult, result);
            Assert.Equal(objCtx, handler.Context);
            Assert.Equal(query, handler.Query);
        }

        [Fact]
        public async Task Correctly_routes_data_through_pipeline_elements()
        {
            Prepare(
                queryBuilder: c => c.Use<SamplePipelineElement<QueryExecutionPayload, object>>()
            );

            var appCtx = new AppContext();
            var objCtx = new ObjContext();
            var query = new SampleQuery();

            var element = Container.Resolve<SamplePipelineElement<QueryExecutionPayload, object>>();

            await QueryExecutor.GetAsync(appCtx, objCtx, query);

            Assert.Equal(appCtx, element.AppContext);
            Assert.Equal(objCtx, element.Data.Context);
            Assert.Equal(query, element.Data.Object);
        }

        [Fact]
        public async Task Creates_obj_context_based_on_app_context_if_needed()
        {
            Prepare();

            var appCtx = new AppContext();
            var query = new SingleInstanceQuery();

            var handler = Container.Resolve<SingleInstanceQueryHandler>();

            await QueryExecutor.GetAsync(appCtx, query);

            Assert.Equal(appCtx, handler.Context.SourceAppContext);
            Assert.Equal(query, handler.Query);
        }
    }
}
