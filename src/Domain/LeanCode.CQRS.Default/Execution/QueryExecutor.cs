using System.Threading.Tasks;
using Autofac;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Execution
{
    public class QueryExecutor<TAppContext> : IQueryExecutor<TAppContext>
        where TAppContext : IPipelineContext
    {
        private readonly PipelineExecutor<TAppContext, QueryExecutionPayload, object> executor;
        private readonly ILifetimeScope lifetimeScope;

        public QueryExecutor(
            IPipelineFactory factory,
            QueryBuilder<TAppContext> config,
            ILifetimeScope lifetimeScope)
        {
            var cfg = Pipeline.Build<TAppContext, QueryExecutionPayload, object>()
                .Configure(new ConfigPipeline<TAppContext, QueryExecutionPayload, object>(config))
                .Finalize<QueryFinalizer<TAppContext>>();

            executor = PipelineExecutor.Create(factory, cfg);
            this.lifetimeScope = lifetimeScope;
        }

        public async Task<TResult> GetAsync<TContext, TResult>(
            TAppContext appContext,
            TContext context,
            IQuery<TContext, TResult> query)
        {
            var payload = new QueryExecutionPayload(context, query);
            var res = await executor
                .ExecuteAsync(appContext, payload)
                .ConfigureAwait(false);
            return (TResult)res;
        }

        public Task<TResult> GetAsync<TContext, TResult>(TAppContext appContext, IQuery<TContext, TResult> query)
        {
            var factory = lifetimeScope.Resolve<IObjectContextFromAppContextFactory<TAppContext, TContext>>();
            var context = factory.Create(appContext);
            return GetAsync(appContext, context, query);
        }
    }
}
