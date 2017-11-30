namespace LeanCode.CQRS
{
    public interface IRemoteQuery<in TContext, out TResult> : IQuery<TContext, TResult>
    { }
}
