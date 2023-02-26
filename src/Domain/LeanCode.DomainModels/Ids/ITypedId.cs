using System.Diagnostics.CodeAnalysis;

namespace LeanCode.DomainModels.Ids;

[SuppressMessage("?", "CA1000", Justification = "Roslyn bug.")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IPrefixedTypedId<TBacking, TSelf> : IEquatable<TSelf>, IComparable<TSelf>
    where TBacking : class
    where TSelf : struct, IPrefixedTypedId<TBacking, TSelf>
{
    TBacking Value { get; }
    bool IsEmpty { get; }

    public static abstract TSelf Parse(TBacking? v);

    [return: NotNullIfNotNull("id")]
    public static abstract TSelf? ParseNullable(TBacking? id);
    public static abstract bool TryParse([NotNullWhen(true)] TBacking? v, out TSelf id);
    public static abstract bool IsValid([NotNullWhen(true)] TBacking? v);
}

[SuppressMessage("?", "CA1000", Justification = "Roslyn bug.")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IRawTypedId<TBacking, TSelf> : IEquatable<TSelf>, IComparable<TSelf>
    where TBacking : struct
    where TSelf : struct, IRawTypedId<TBacking, TSelf>
{
    TBacking Value { get; }
    bool IsEmpty { get; }

    public static abstract TSelf Parse(TBacking? v);

    [return: NotNullIfNotNull("id")]
    public static abstract TSelf? ParseNullable(TBacking? id);
    public static abstract bool TryParse([NotNullWhen(true)] TBacking? v, out TSelf id);
    public static abstract bool IsValid([NotNullWhen(true)] TBacking? v);
}
