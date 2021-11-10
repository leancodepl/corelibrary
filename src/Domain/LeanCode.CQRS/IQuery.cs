namespace LeanCode.CQRS
{
    /// <summary>
    /// Marker interface, do not use directly.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1040", Justification = "Marker interface.")]
    public interface IQuery { }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1040", Justification = "Marker interface.")]
    public interface IQuery<out TResult> : IQuery { }
}
