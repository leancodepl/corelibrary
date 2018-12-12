namespace LeanCode.CQRS
{
    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    public interface IQuery
    { }

    public interface IQuery<in TContext, out TResult> : IQuery
    { }
}
