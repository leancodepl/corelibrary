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
            if (typeof(TContext) == typeof(ObjContext))
            {
                var ctx = new ObjContextFromAppContextFactory().Create(appContext);
                return GetAsync(appContext, ctx, (IQuery<ObjContext, TResult>)query);
            }
            else if (typeof(TContext) == typeof(ObjContextWoCtor))
            {
                var ctx = new ObjContextWoCtorFromAppContextFactory().Create(appContext);
                return GetAsync(appContext, ctx, (IQuery<ObjContextWoCtor, TResult>)query);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
