using System.Diagnostics.CodeAnalysis;

namespace LeanCode.DomainModels.Model;

/// <summary>A stronly typed id consisting of an ulid prefixed with a lower-cased name of an entity.</summary>
/// <remarks>Use <see cref="IdSlugAttribute"/> to override default entity prefix.</remarks>
[Serialization.TypedIdConverter]
[System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]
[SuppressMessage("?", "CA1000", Justification = "The methods are expected.")]
[SuppressMessage("?", "CA1036", Justification = "We don't want to have easy comparison as it might be abused.")]
public readonly record struct SUlid<TEntity> : IEquatable<SUlid<TEntity>>, IComparable<SUlid<TEntity>>
    where TEntity : class, IIdentifiable<SUlid<TEntity>>
{
    private const int UlidLength = 26;
    private const char Separator = '_';
    private static readonly string TypeName = SIdExtensions.GetPrefix<TEntity>();

    public static readonly int RawLength = UlidLength + 1 + TypeName.Length;
    public static readonly SUlid<TEntity> Empty = new(Ulid.Empty);

    private readonly string? value;

    public string Value => value ?? Empty.Value;

    private SUlid(string str)
    {
        value = str;
    }

    public SUlid(Ulid ulid)
    {
        value = string.Create(
            RawLength,
            ulid,
            static (span, ulid) =>
            {
                TypeName.CopyTo(span);
                span[TypeName.Length] = Separator;
                ulid.TryWriteStringify(span[(TypeName.Length + 1)..]);
            });
    }

    public static SUlid<TEntity> New() => new(Ulid.NewUlid());

    [return: NotNullIfNotNull("id")]
    public static SUlid<TEntity>? FromNullableString(string? id) => id is string v ? FromString(v) : null;

    public static bool TryParse(string? v, out SUlid<TEntity> id)
    {
        if (IsValid(v))
        {
            id = new SUlid<TEntity>(v);
            return true;
        }
        else
        {
            id = default;
            return false;
        }
    }

    public static SUlid<TEntity> FromString(string v)
    {
        if (IsValid(v))
        {
            return new SUlid<TEntity>(v);
        }
        else
        {
            throw new FormatException($"The id has invalid format. It should look like {TypeName}{Separator}(random id).");
        }
    }

    public static SUlid<TEntity> FromUlidOrSUlid(string v)
    {
        if (TryParse(v, out var sulid))
        {
            return sulid;
        }
        else if (Ulid.TryParse(v, out var ulid))
        {
            return new(ulid);
        }
        else
        {
            throw new FormatException($"The id cannot be parsed to SUlid nor Ulid.");
        }
    }

    public static bool TryParseFromUlidOrSUlid([NotNullWhen(true)] string? v, out SUlid<TEntity> id)
    {
        if (IsValid(v))
        {
            id = new SUlid<TEntity>(v!);
            return true;
        }
        else if (Ulid.TryParse(v, out var ulid))
        {
            id = new(ulid);
            return true;
        }
        else
        {
            id = default;
            return false;
        }
    }

    public static bool IsValid([NotNullWhen(true)] string? v)
    {
        if (v is null)
        {
            return false;
        }
        else
        {
            var span = v.AsSpan();
            return
                span.Length == RawLength &&
                span.StartsWith(TypeName) &&
                span[TypeName.Length] == Separator &&
                Ulid.TryParse(span[(TypeName.Length + 1)..], out _);
        }
    }

    public bool Equals(SUlid<TEntity> other) => Value.Equals(other.Value, StringComparison.Ordinal);

    public int CompareTo(SUlid<TEntity> other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Value;

    public static implicit operator string(SUlid<TEntity> id) => id.Value;
}
