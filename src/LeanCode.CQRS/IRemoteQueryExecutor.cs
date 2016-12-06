using System.Threading.Tasks;

namespace LeanCode.CQRS
{
    public interface IRemoteQueryExecutor
    {
        Task<TResult> ExecuteQuery<TResult>(IRemoteQuery<TResult> query);
    }
}
