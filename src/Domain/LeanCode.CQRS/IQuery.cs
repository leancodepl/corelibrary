namespace LeanCode.CQRS
{
    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IQuery
    { }

    /// <summary>
    /// Represents a query
    /// </summary>
    /// <typeparam name="TContext">The object context of a query </typeparam>
    /// <typeparam name="TResult">The result of a query </typeparam>
    public interface IQuery<in TContext, out TResult> : IQuery
    { }

    public interface INoContextQuery<out TResult> : IQuery<VoidContext, TResult>
    { }
}
