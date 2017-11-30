namespace LeanCode.CQRS
{
    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface ICommand
    { }

    public interface ICommand<in TContext> : ICommand
    { }

    public interface INoContextCommand : ICommand<VoidContext>
    { }
}
