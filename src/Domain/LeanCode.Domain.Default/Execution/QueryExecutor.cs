using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.Domain.Default.Execution
{
    using Executor = PipelineExecutor<ExecutionContext, IQuery, object>;

    public class QueryExecutor : IQueryExecutor
    {
        private readonly Executor executor;

        public QueryExecutor(
            IPipelineFactory factory,
            QueryBuilder config)
        {
            var cfg = Pipeline.Build<ExecutionContext, IQuery, object>()
                .Configure(new ConfigPipeline<ExecutionContext, IQuery, object>(config))
                .Finalize<QueryFinalizer<ExecutionContext>>();

            executor = PipelineExecutor.Create(factory, cfg);
        }

        public async Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
        {
            var res = await executor
                .ExecuteAsync(new ExecutionContext(), query)
                .ConfigureAwait(false);
            return (TResult)res;
        }
    }
}
