namespace LeanCode.CQRS
{
    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IOperation { }

    public interface IOperation<out TResult> : IOperation { }
}
