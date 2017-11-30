namespace LeanCode.CQRS
{
    public interface IRemoteCommand<in TContext> : ICommand<TContext>
    { }
}
