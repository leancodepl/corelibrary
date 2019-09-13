using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Wrappers
{
    internal class QueryHandlerWrapper<TAppContext, TQuery, TResult> : IQueryHandlerWrapper
        where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TAppContext, TQuery, TResult> handler;

        public QueryHandlerWrapper(IQueryHandler<TAppContext, TQuery, TResult> handler)
        {
            this.handler = handler;
        }

        public async Task<object?> ExecuteAsync(object context, IQuery query)
        {
            return await handler
                .ExecuteAsync((TAppContext)context, (TQuery)query)
                .ConfigureAwait(false);
        }
    }
}
