namespace LeanCode.CQRS
{
    public interface IRemoteQuery<out TResult> : IQuery<TResult> { }
}
