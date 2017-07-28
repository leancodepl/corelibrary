using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Execution
{
    public class QueryExecutor<TAppContext> : IQueryExecutor<TAppContext>
        where TAppContext : IPipelineContext
    {
        private readonly PipelineExecutor<TAppContext, IQuery, object> executor;

        public QueryExecutor(
            IPipelineFactory factory,
            QueryBuilder<TAppContext> config)
        {
            var cfg = Pipeline.Build<TAppContext, IQuery, object>()
                .Configure(new ConfigPipeline<TAppContext, IQuery, object>(config))
                .Finalize<QueryFinalizer<TAppContext>>();

            executor = PipelineExecutor.Create(factory, cfg);
        }

        public async Task<TResult> GetAsync<TResult>(
            TAppContext context, IQuery<TResult> query)
        {
            var res = await executor
                .ExecuteAsync(context, query)
                .ConfigureAwait(false);
            return (TResult)res;
        }
    }
}
