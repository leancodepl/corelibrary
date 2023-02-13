using System.Diagnostics.CodeAnalysis;

namespace LeanCode.DomainModels.Model;

[Serialization.TypedIdConverter]
[System.Diagnostics.DebuggerDisplay("{Value,nq}")]
[SuppressMessage("?", "CA1000", Justification = "The methods are expected.")]
[SuppressMessage("?", "CA1036", Justification = "We don't want to have easy comparison as it might be abused.")]
public readonly record struct Ulid<TEntity>(Ulid Value) : IComparable<Ulid<TEntity>>
    where TEntity : class, IIdentifiable<Ulid<TEntity>>
{
    public static readonly Ulid<TEntity> Empty;

    public static Ulid<TEntity> FromUlid(Ulid id) => new(id);
    public static Ulid<TEntity>? FromUlid(Ulid? id) => id is Ulid v ? new Ulid<TEntity>(v) : null;

    public static Ulid<TEntity> New() => new(Ulid.NewUlid());

    public static Ulid<TEntity> Parse(ReadOnlySpan<char> base32) => new(Ulid.Parse(base32));

    public static Ulid<TEntity> Parse(string base32) => new(Ulid.Parse(base32));

    public static bool TryParse(ReadOnlySpan<char> base32, out Ulid<TEntity>? ulid) =>
        (Ulid.TryParse(base32, out var u) ? ulid = new(u) : ulid = default) is not null;

    public static bool TryParse([NotNullWhen(true)] string? base32, out Ulid<TEntity>? ulid) =>
        (Ulid.TryParse(base32, out var u) ? ulid = new(u) : ulid = default) is not null;

    public int CompareTo(Ulid<TEntity> other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();

    public Ulid ToUlid() => Value;

    public static implicit operator Ulid(Ulid<TEntity> id) => id.Value;

    public static explicit operator Ulid<TEntity>(Ulid id) => new(id);
}
