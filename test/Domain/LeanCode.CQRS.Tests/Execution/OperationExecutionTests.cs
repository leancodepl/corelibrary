using System.Threading.Tasks;
using Autofac;
using LeanCode.Contracts;
using Xunit;

namespace LeanCode.CQRS.Tests;

public class OperationExecutionTests : BaseCQRSTests
{
    [Fact]
    public async Task Executes_operation_handler_with_correct_data()
    {
        Prepare();

        var appCtx = new AppContext();
        var operation = new SingleInstanceOperation();
        var expResult = new object();
        var handler = Container.Resolve<SingleInstanceOperationHandler>();
        handler.Result = expResult;

        var result = await OperationExecutor.ExecuteAsync(appCtx, operation);

        Assert.Equal(expResult, result);
        Assert.Equal(appCtx, handler.Context);
        Assert.Equal(operation, handler.Operation);
    }

    [Fact]
    public async Task Correctly_routes_data_through_pipeline_elements()
    {
        Prepare(operationBuilder: c => c.Use<SamplePipelineElement<IOperation, object>>());

        var appCtx = new AppContext();
        var operation = new SampleOperation();

        var element = Container.Resolve<SamplePipelineElement<IOperation, object>>();

        await OperationExecutor.ExecuteAsync(appCtx, operation);

        Assert.Equal(appCtx, element.AppContext);
        Assert.Equal(operation, element.Data);
    }

    [Fact]
    public async Task Creates_obj_context_based_on_app_context_if_needed()
    {
        Prepare();

        var appCtx = new AppContext();
        var operation = new SingleInstanceOperation();

        var handler = Container.Resolve<SingleInstanceOperationHandler>();

        await OperationExecutor.ExecuteAsync(appCtx, operation);

        Assert.Equal(appCtx, handler.Context);
        Assert.Equal(operation, handler.Operation);
    }
}
