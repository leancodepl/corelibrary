using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public sealed class QueryFinalizer<TAppContext> : IPipelineFinalizer<TAppContext, IQuery, object?>
        where TAppContext : notnull, IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<QueryFinalizer<TAppContext>>();
        private readonly IQueryHandlerResolver<TAppContext> resolver;

        public QueryFinalizer(IQueryHandlerResolver<TAppContext> resolver)
        {
            this.resolver = resolver;
        }

        public async Task<object?> ExecuteAsync(TAppContext appContext, IQuery query)
        {
            var queryType = query.GetType();
            var handler = resolver.FindQueryHandler(queryType);

            if (handler is null)
            {
                logger.Fatal("Cannot find a handler for query {@Query}", query);

                throw new QueryHandlerNotFoundException(queryType);
            }

            return await handler.ExecuteAsync(appContext, query);
        }
    }
}
