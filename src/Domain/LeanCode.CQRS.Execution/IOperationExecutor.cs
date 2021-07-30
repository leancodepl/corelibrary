using System.Threading.Tasks;

namespace LeanCode.CQRS.Execution
{
    public interface IOperationExecutor<TAppContext>
    {
        Task<TResult> ExecuteAsync<TResult>(TAppContext context, IOperation<TResult> operation);
    }
}
