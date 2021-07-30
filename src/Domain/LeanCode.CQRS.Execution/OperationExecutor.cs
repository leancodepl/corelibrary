using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public delegate PipelineBuilder<TAppContext, IOperation, object?> OperationBuilder<TAppContext>(
        PipelineBuilder<TAppContext, IOperation, object?> builder)
        where TAppContext : IPipelineContext;

    public class OperationExecutor<TAppContext> : IOperationExecutor<TAppContext>
        where TAppContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<OperationExecutor<TAppContext>>();

        private readonly PipelineExecutor<TAppContext, IOperation, object?> executor;

        public OperationExecutor(IPipelineFactory factory, OperationBuilder<TAppContext> config)
        {
            executor = PipelineExecutor.Create(factory, Pipeline
                .Build<TAppContext, IOperation, object?>()
                .Configure(new ConfigPipeline<TAppContext, IOperation, object?>(config))
                .Finalize<OperationFinalizer<TAppContext>>());
        }

        public async Task<TResult> ExecuteAsync<TResult>(TAppContext appContext, IOperation<TResult> operation)
        {
            var res = await executor.ExecuteAsync(appContext, operation);
            logger.Information("Operation {@Operation} executed successfully", operation);
            return (TResult)res!;
        }
    }
}
