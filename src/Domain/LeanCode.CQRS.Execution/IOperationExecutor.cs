using System.Threading.Tasks;
using LeanCode.Contracts;

namespace LeanCode.CQRS.Execution
{
    public interface IOperationExecutor<TAppContext>
    {
        Task<TResult> ExecuteAsync<TResult>(TAppContext appContext, IOperation<TResult> operation);
    }
}
