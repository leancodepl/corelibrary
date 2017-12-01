using System.Threading.Tasks;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public interface IRemoteQueryExecutor
    {
        Task<TOutput> GetAsync<TContext, TOutput>(IRemoteQuery<TContext, TOutput> query);
    }
}
