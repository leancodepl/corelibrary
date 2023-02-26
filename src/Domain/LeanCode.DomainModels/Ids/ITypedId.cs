using System.Diagnostics.CodeAnalysis;

namespace LeanCode.DomainModels.Ids;

[SuppressMessage("?", "CA1000", Justification = "Roslyn bug.")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IPrefixedTypedId<TSelf> : IEquatable<TSelf>, IComparable<TSelf>
    where TSelf : struct, IPrefixedTypedId<TSelf>
{
    string Value { get; }
    public static abstract int RawLength { get; }
    public static abstract TSelf Parse(string? v);
    public static abstract System.Linq.Expressions.Expression<Func<string, TSelf>> FromDatabase { get; }
}

[SuppressMessage("?", "CA1000", Justification = "Roslyn bug.")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IRawTypedId<TBacking, TSelf> : IEquatable<TSelf>, IComparable<TSelf>
    where TBacking : struct
    where TSelf : struct, IRawTypedId<TBacking, TSelf>
{
    TBacking Value { get; }
    public static abstract TSelf Parse(TBacking? v);
    public static abstract System.Linq.Expressions.Expression<Func<TBacking, TSelf>> FromDatabase { get; }
}
