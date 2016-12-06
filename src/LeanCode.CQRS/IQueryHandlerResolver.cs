namespace LeanCode.CQRS
{
    public interface IQueryHandlerResolver
    {
        IQueryHandlerWrapper<TResult> FindQueryHandler<TResult>(IQuery<TResult> query);
    }
}
