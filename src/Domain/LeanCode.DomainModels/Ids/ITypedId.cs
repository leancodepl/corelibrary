using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;

namespace LeanCode.DomainModels.Ids;

[SuppressMessage("?", "CA1000", Justification = "Roslyn bug.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPrefixedTypedId<TSelf>
    : IEquatable<TSelf>,
        IComparable<TSelf>,
        ISpanFormattable,
        IUtf8SpanFormattable,
        IEqualityOperators<TSelf, TSelf, bool>,
        IHasEmptyId<TSelf>
    where TSelf : struct, IPrefixedTypedId<TSelf>
{
    string Value { get; }
    static abstract TSelf Parse(string v);
    static abstract bool IsValid(string? v);

    [EditorBrowsable(EditorBrowsableState.Never)]
    static abstract Expression<Func<string, TSelf>> FromDatabase { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    static abstract Expression<Func<TSelf, TSelf, bool>> DatabaseEquals { get; }
}

[SuppressMessage("?", "CA1000", Justification = "Roslyn bug.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IRawTypedId<TBacking, TSelf>
    : IEquatable<TSelf>,
        IComparable<TSelf>,
        ISpanFormattable,
        IUtf8SpanFormattable,
        IEqualityOperators<TSelf, TSelf, bool>,
        IHasEmptyId<TSelf>
    where TBacking : struct
    where TSelf : struct, IRawTypedId<TBacking, TSelf>
{
    TBacking Value { get; }
    static abstract TSelf Parse(TBacking v);

    [EditorBrowsable(EditorBrowsableState.Never)]
    static abstract Expression<Func<TBacking, TSelf>> FromDatabase { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    static abstract Expression<Func<TSelf, TSelf, bool>> DatabaseEquals { get; }
}

[SuppressMessage("?", "CA1000", Justification = "Roslyn bug.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IRawStringTypedId<TSelf>
    : IEquatable<TSelf>,
        IComparable<TSelf>,
        ISpanFormattable,
        IUtf8SpanFormattable,
        IEqualityOperators<TSelf, TSelf, bool>,
        IHasEmptyId<TSelf>
    where TSelf : struct, IRawStringTypedId<TSelf>
{
    string Value { get; }
    static abstract TSelf Parse(string v);
    static abstract bool IsValid(string? v);

    [EditorBrowsable(EditorBrowsableState.Never)]
    static abstract Expression<Func<string, TSelf>> FromDatabase { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    static abstract Expression<Func<TSelf, TSelf, bool>> DatabaseEquals { get; }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IHasEmptyId<TSelf>
    where TSelf : struct, IHasEmptyId<TSelf>
{
    static abstract TSelf Empty { get; }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IConstSizeTypedId
{
    static abstract int RawLength { get; }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IMaxLengthTypedId
{
    static abstract int MaxLength { get; }
}
