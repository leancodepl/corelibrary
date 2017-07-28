using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class StubQueryExecutor : IQueryExecutor<AppContext>
    {
        public AppContext LastContext { get; private set; }
        public object LastQuery { get; private set; }

        public Task<TResult> GetAsync<TResult>(
            AppContext context, IQuery<TResult> query)
        {
            LastContext = context;
            LastQuery = query;
            return Task.FromResult(default(TResult));
        }
    }
}
