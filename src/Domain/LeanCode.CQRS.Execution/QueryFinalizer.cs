using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public sealed class QueryFinalizer<TAppContext>
        : IPipelineFinalizer<TAppContext, QueryExecutionPayload, object>
        where TAppContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<QueryFinalizer<TAppContext>>();
        private readonly IQueryHandlerResolver<TAppContext> resolver;

        public QueryFinalizer(IQueryHandlerResolver<TAppContext> resolver)
        {
            this.resolver = resolver;
        }

        public async Task<object> ExecuteAsync(
            TAppContext _, QueryExecutionPayload payload)
        {
            var context = payload.Context;
            var query = payload.Object;

            var queryType = query.GetType();
            var handler = resolver.FindQueryHandler(queryType);
            if (handler is null)
            {
                logger.Fatal("Cannot find a handler for query {@Query}", query);
                throw new QueryHandlerNotFoundException(queryType);
            }

            object result;
            try
            {
                result = await handler.ExecuteAsync(context, query).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error(
                    ex, "Cannot execute query {@Query} because of internal error",
                    query);
                throw;
            }
            logger.Information("Query {@Query} executed successfuly", query);
            return result;
        }
    }
}
