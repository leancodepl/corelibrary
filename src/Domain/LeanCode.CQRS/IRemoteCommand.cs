namespace LeanCode.CQRS
{
    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IRemoteCommand : ICommand
    { }

    public interface IRemoteCommand<in TContext> : IRemoteCommand, ICommand<TContext>
    { }
}
