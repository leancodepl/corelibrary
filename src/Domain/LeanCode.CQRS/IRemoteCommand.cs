namespace LeanCode.CQRS
{
    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IRemoteCommand : ICommand
    { }

    /// <summary>
    /// A command that is available to clients via RemoteCQRS
    /// </summary>
    public interface IRemoteCommand<in TContext> : IRemoteCommand, ICommand<TContext>
    { }
}
