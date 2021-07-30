namespace LeanCode.CQRS
{
    public interface IRemoteOperation<out TResult> : IOperation<TResult> { }
}
