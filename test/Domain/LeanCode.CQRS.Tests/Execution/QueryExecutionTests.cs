using System.Threading.Tasks;
using Autofac;
using LeanCode.Contracts;
using Xunit;

namespace LeanCode.CQRS.Tests;

public class QueryExecutionTests : BaseCQRSTests
{
    [Fact]
    public async Task Executes_query_handler_with_correct_data()
    {
        Prepare();

        var appCtx = new AppContext();
        var query = new SingleInstanceQuery();
        var expResult = new object();
        var handler = Container.Resolve<SingleInstanceQueryHandler>();
        handler.Result = expResult;

        var result = await QueryExecutor.GetAsync(appCtx, query);

        Assert.Equal(expResult, result);
        Assert.Equal(appCtx, handler.Context);
        Assert.Equal(query, handler.Query);
    }

    [Fact]
    public async Task Correctly_routes_data_through_pipeline_elements()
    {
        Prepare(queryBuilder: c => c.Use<SamplePipelineElement<IQuery, object>>());

        var appCtx = new AppContext();
        var query = new SampleQuery();

        var element = Container.Resolve<SamplePipelineElement<IQuery, object>>();

        await QueryExecutor.GetAsync(appCtx, query);

        Assert.Equal(appCtx, element.AppContext);
        Assert.Equal(query, element.Data);
    }

    [Fact]
    public async Task Creates_obj_context_based_on_app_context_if_needed()
    {
        Prepare();

        var appCtx = new AppContext();
        var query = new SingleInstanceQuery();

        var handler = Container.Resolve<SingleInstanceQueryHandler>();

        await QueryExecutor.GetAsync(appCtx, query);

        Assert.Equal(appCtx, handler.Context);
        Assert.Equal(query, handler.Query);
    }
}
