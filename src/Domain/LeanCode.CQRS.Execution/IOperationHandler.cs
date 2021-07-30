using System.Threading.Tasks;

namespace Domain.LeanCode.CQRS.Execution
{
    public interface IOperationHandler<in TContext, in TOperation, TResult>
        where TOperation : IOperation<TResult>
    {
        public Task<TResult> ExecuteAsync(TContext context, TOperation operation);
    }
}
