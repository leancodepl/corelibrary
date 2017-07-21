using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;

namespace LeanCode.Domain.Default
{
    class QueryHandlerWrapper<TQuery, TResult> : IQueryHandlerWrapper
        where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> handler;

        public QueryHandlerWrapper(IQueryHandler<TQuery, TResult> handler)
        {
            this.handler = handler;
        }

        public async Task<object> ExecuteAsync(IQuery query)
        {
            return await handler.ExecuteAsync((TQuery)query).ConfigureAwait(false);
        }
    }
}
