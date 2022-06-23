using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public delegate PipelineBuilder<TAppContext, IQuery, object?> QueryBuilder<TAppContext>(
        PipelineBuilder<TAppContext, IQuery, object?> builder)
        where TAppContext : IPipelineContext;

    public class QueryExecutor<TAppContext> : IQueryExecutor<TAppContext>
        where TAppContext : notnull, IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<QueryExecutor<TAppContext>>();

        private readonly PipelineExecutor<TAppContext, IQuery, object?> executor;

        public QueryExecutor(IPipelineFactory factory, QueryBuilder<TAppContext> config)
        {
            executor = PipelineExecutor.Create(factory, Pipeline
                .Build<TAppContext, IQuery, object?>()
                .Configure(new ConfigPipeline<TAppContext, IQuery, object?>(config))
                .Finalize<QueryFinalizer<TAppContext>>());
        }

        public async Task<TResult> GetAsync<TResult>(TAppContext appContext, IQuery<TResult> query)
        {
            var res = await executor.ExecuteAsync(appContext, query);
            logger.Information("Query {@Query} executed successfully", query);
            return (TResult)res!;
        }
    }
}
