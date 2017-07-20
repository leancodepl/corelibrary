using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    using Executor = PipelineExecutor<IQuery, object>;
    using Builder = PipelineBuilder<IQuery, object>;

    public class QueryExecutor : IQueryExecutor
    {
        private readonly Executor executor;

        public QueryExecutor(
            IPipelineFactory factory,
            Func<Builder, Builder> config)
        {
            var cfg = Pipeline.Build<IQuery, object>()
                .Configure(config)
                .Finalize<QueryFinalizer>();

            executor = PipelineExecutor.Create(factory, cfg);
        }

        public async Task<TResult> GetAsync<TResult>(IQuery<TResult> query)
        {
            var res = await executor.ExecuteAsync(query).ConfigureAwait(false);
            return (TResult)res;
        }
    }
}
