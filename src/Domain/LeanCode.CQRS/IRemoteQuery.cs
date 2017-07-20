namespace LeanCode.CQRS
{
    /// <summary>
    ///  Marker interface, do not use directly.
    /// </summary>
    public interface IRemoteQuery
    { }

    public interface IRemoteQuery<TResult> : IQuery<TResult>, IRemoteQuery
    { }
}
