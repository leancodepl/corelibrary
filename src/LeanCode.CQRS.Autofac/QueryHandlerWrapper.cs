using System.Threading.Tasks;

namespace LeanCode.CQRS.Autofac
{
    public class QueryHandlerWrapper<TQuery, TResult> : IQueryHandlerWrapper<TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> handler;

        public QueryHandlerWrapper(IQueryHandler<TQuery, TResult> handler)
        {
            this.handler = handler;
        }

        public Task<TResult> ExecuteAsync(IQuery<TResult> query)
        {
            return handler.ExecuteAsync((TQuery)query);
        }
    }
}
