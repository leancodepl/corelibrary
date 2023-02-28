using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace LeanCode.DomainModels.Ids;

[SuppressMessage("?", "CA1000", Justification = "Roslyn bug.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPrefixedTypedId<TSelf> : IEquatable<TSelf>, IComparable<TSelf>
    where TSelf : struct, IPrefixedTypedId<TSelf>
{
    string Value { get; }
    public static abstract int RawLength { get; }
    public static abstract TSelf Parse(string? v);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static abstract Expression<Func<string, TSelf>> FromDatabase { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static abstract Expression<Func<TSelf, TSelf, bool>> DatabaseEquals { get; }
}

[SuppressMessage("?", "CA1000", Justification = "Roslyn bug.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IRawTypedId<TBacking, TSelf> : IEquatable<TSelf>, IComparable<TSelf>
    where TBacking : struct
    where TSelf : struct, IRawTypedId<TBacking, TSelf>
{
    TBacking Value { get; }
    public static abstract TSelf Parse(TBacking? v);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static abstract Expression<Func<TBacking, TSelf>> FromDatabase { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static abstract Expression<Func<TSelf, TSelf, bool>> DatabaseEquals { get; }
}
