using System.Threading.Tasks;

namespace LeanCode.CQRS
{
    public interface IRemoteQueryExecutor
    {
        Task<TResult> ExecuteAsync<TResult>(IRemoteQuery<TResult> query);
    }
}
