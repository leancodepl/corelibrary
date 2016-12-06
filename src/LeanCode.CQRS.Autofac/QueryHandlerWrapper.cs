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

        public TResult Execute(IQuery<TResult> query)
        {
            return handler.Execute((TQuery)query);
        }
    }
}
