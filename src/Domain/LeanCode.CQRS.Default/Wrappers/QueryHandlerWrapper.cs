using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Default.Wrappers
{
    class QueryHandlerWrapper<TContext, TQuery, TResult> : IQueryHandlerWrapper
        where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TContext, TQuery, TResult> handler;

        public QueryHandlerWrapper(IQueryHandler<TContext, TQuery, TResult> handler)
        {
            this.handler = handler;
        }

        public async Task<object> ExecuteAsync(object context, IQuery query)
        {
            return await handler
                .ExecuteAsync((TContext)context, (TQuery)query)
                .ConfigureAwait(false);
        }
    }
}
