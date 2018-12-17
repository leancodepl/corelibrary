using System.Threading.Tasks;
using Autofac;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Execution
{
    public class QueryExecutor<TAppContext> : IQueryExecutor<TAppContext>
        where TAppContext : IPipelineContext
    {
        private readonly PipelineExecutor<TAppContext, IQuery, object> executor;
        private readonly ILifetimeScope lifetimeScope;

        public QueryExecutor(
            IPipelineFactory factory,
            QueryBuilder<TAppContext> config,
            ILifetimeScope lifetimeScope)
        {
            var cfg = Pipeline.Build<TAppContext, IQuery, object>()
                .Configure(new ConfigPipeline<TAppContext, IQuery, object>(config))
                .Finalize<QueryFinalizer<TAppContext>>();

            executor = PipelineExecutor.Create(factory, cfg);
            this.lifetimeScope = lifetimeScope;
        }

        public async Task<TResult> GetAsync<TResult>(
            TAppContext appContext,
            IQuery<TResult> query)
        {
            var res = await executor
                .ExecuteAsync(appContext, query)
                .ConfigureAwait(false);
            return (TResult)res;
        }
    }
}
