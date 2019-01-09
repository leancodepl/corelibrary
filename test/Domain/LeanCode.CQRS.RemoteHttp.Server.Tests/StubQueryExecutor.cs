using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class StubQueryExecutor : IQueryExecutor<AppContext>
    {
        public AppContext LastAppContext { get; private set; }
        public IQuery LastQuery { get; private set; }

        public Task<TResult> GetAsync<TResult>(
            AppContext appContext,
            IQuery<TResult> query)
        {
            LastAppContext = appContext;
            LastQuery = query;
            return Task.FromResult(default(TResult));
        }
    }
}
