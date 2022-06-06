using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public class OperationFinalizer<TAppContext> : IPipelineFinalizer<TAppContext, IOperation, object?>
        where TAppContext : notnull, IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<OperationFinalizer<TAppContext>>();
        private readonly IOperationHandlerResolver<TAppContext> resolver;

        public OperationFinalizer(IOperationHandlerResolver<TAppContext> resolver)
        {
            this.resolver = resolver;
        }

        public async Task<object?> ExecuteAsync(TAppContext appContext, IOperation operation)
        {
            var operationType = operation.GetType();
            var handler = resolver.FindOperationHandler(operationType);

            if (handler is null)
            {
                logger.Fatal("Cannot find a handler for operation {@Operation}", operation);

                throw new OperationHandlerNotFoundException(operationType);
            }

            return await handler.ExecuteAsync(appContext, operation);
        }
    }
}
