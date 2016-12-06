namespace LeanCode.CQRS
{
    public interface IQueryCacheKeyProvider
    {
        string GetKey<TResult>(IQuery<TResult> query);
    }
}
