namespace LeanCode.CQRS
{
    public interface IQueryExecutor
    {
        TResult Execute<TResult>(IQuery<TResult> query);
    }
}
