namespace LeanCode.CQRS
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1040", Justification = "Marker interface.")]
    public interface IRemoteQuery<out TResult> : IQuery<TResult> { }
}
