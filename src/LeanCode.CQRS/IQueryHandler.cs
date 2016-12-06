namespace LeanCode.CQRS
{
    public interface IQueryHandler<in TQuery, out TResult>
        where TQuery : IQuery<TResult>
    {
        TResult Execute(TQuery query);
    }

    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IQueryHandlerWrapper<TResult>
    {
        TResult Execute(IQuery<TResult> query);
    }
}
