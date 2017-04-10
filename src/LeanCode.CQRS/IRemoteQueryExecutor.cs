using System.Threading.Tasks;

namespace LeanCode.CQRS
{
    public interface IRemoteQueryExecutor
    {
        Task<TResult> GetAsync<TResult>(IRemoteQuery<TResult> query);
    }
}
