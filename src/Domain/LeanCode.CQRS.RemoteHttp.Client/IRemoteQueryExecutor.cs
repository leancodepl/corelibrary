using System.Threading.Tasks;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public interface IRemoteQueryExecutor
    {
        Task<TOutput> GetAsync<TOutput>(IRemoteQuery<TOutput> query);
    }
}
