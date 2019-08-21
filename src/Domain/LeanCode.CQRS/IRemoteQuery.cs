namespace LeanCode.CQRS
{
    /// <summary>
    /// An <see cref="IQuery{TContext, TResult}"/> that is available to clients via RemoteCQRS
    /// </summary>
    public interface IRemoteQuery<in TContext, out TResult> : IQuery<TContext, TResult>
    { }
}
