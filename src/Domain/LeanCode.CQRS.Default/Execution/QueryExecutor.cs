using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Execution
{
    public class QueryExecutor<TAppContext> : IQueryExecutor<TAppContext>
        where TAppContext : IPipelineContext
    {
        private readonly PipelineExecutor<TAppContext, QueryExecutionPayload, object> executor;

        public QueryExecutor(
            IPipelineFactory factory,
            QueryBuilder<TAppContext> config)
        {
            var cfg = Pipeline.Build<TAppContext, QueryExecutionPayload, object>()
                .Configure(new ConfigPipeline<TAppContext, QueryExecutionPayload, object>(config))
                .Finalize<QueryFinalizer<TAppContext>>();

            executor = PipelineExecutor.Create(factory, cfg);
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
    }
}
