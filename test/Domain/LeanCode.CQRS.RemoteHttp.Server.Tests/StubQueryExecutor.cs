using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class StubQueryExecutor : IQueryExecutor<AppContext>
    {
        public AppContext LastAppContext { get; private set; }
        public object LastContext { get; private set; }
        public IQuery LastQuery { get; private set; }

        public Task<TResult> GetAsync<TContext, TResult>(
            AppContext appContext,
            TContext context,
            IQuery<TContext, TResult> query)
        {
            LastAppContext = appContext;
            LastContext = context;
            LastQuery = query;
            return Task.FromResult(default(TResult));
        }

        public Task<TResult> GetAsync<TContext, TResult>(
            AppContext appContext,
            IQuery<TContext, TResult> query)
        {
            throw new NotImplementedException();
        }
    }
}
