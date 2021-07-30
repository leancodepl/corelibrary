using System.Threading.Tasks;

namespace Domain.LeanCode.CQRS.Execution
{
    public interface IOperationExecutor<TAppContext>
    {
        Task<TResult> ExecuteAsync<TResult>(TAppContext context, IOperation<TResult> operation);
    }
}
