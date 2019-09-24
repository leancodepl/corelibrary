using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Execution
{
    public class QueryExecutor<TAppContext> : IQueryExecutor<TAppContext>
        where TAppContext : notnull, IPipelineContext
    {
        private readonly PipelineExecutor<TAppContext, IQuery, object?> executor;

        public QueryExecutor(IPipelineFactory factory, QueryBuilder<TAppContext> config)
        {
            executor = PipelineExecutor.Create(factory, Pipeline
                .Build<TAppContext, IQuery, object?>()
                .Configure(new ConfigPipeline<TAppContext, IQuery, object?>(config))
                .Finalize<QueryFinalizer<TAppContext>>());
        }

        [return: MaybeNull]
        public async Task<TResult> GetAsync<TResult>(TAppContext appContext, IQuery<TResult> query)
        {
            var res = await executor.ExecuteAsync(appContext, query);
            return (TResult)res!;
        }
    }
}
