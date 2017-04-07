using System.Threading.Tasks;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class StubQueryExecutor : IQueryExecutor
    {
        public object LastQuery { get; private set; }

        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
        {
            LastQuery = query;
            return Task.FromResult(default(TResult));
        }
    }
}
