using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Wrappers
{
    internal class OperationHandlerWrapper<TAppContext, TOperation, TResult> : IOperationHandlerWrapper
        where TOperation : IOperation<TResult>
    {
        private readonly IOperationHandler<TAppContext, TOperation, TResult> handler;

        public OperationHandlerWrapper(IOperationHandler<TAppContext, TOperation, TResult> handler)
        {
            this.handler = handler;
        }

        public async Task<object?> ExecuteAsync(object context, IOperation operation)
        {
            // explicit async to cast the result
            return await handler.ExecuteAsync((TAppContext)context, (TOperation)operation);
        }
    }
}
