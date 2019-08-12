namespace LeanCode.CQRS
{
    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface ICommand
    { }

    /// <summary>
    /// Represents a command
    /// </summary>
    /// <typeparam name="TContext">The object context of a command </typeparam>
    public interface ICommand<in TContext> : ICommand
    { }

    public interface INoContextCommand : ICommand<VoidContext>
    { }
}
