namespace LeanCode.CQRS
{
    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IRemoteCommand : ICommand
    { }

    /// <summary>
    /// An <see cref="ICommand{TContext}"/> that is available to clients via RemoteCQRS
    /// </summary>
    public interface IRemoteCommand<in TContext> : IRemoteCommand, ICommand<TContext>
    { }
}
