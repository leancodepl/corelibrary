namespace LeanCode.CQRS
{
    /// <summary>
    /// A query that is available to clients via RemoteCQRS
    /// </summary>
    public interface IRemoteQuery<in TContext, out TResult> : IQuery<TContext, TResult>
    { }
}
