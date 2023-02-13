using System.Diagnostics.CodeAnalysis;
using LeanCode.DomainModels.Serialization;

namespace LeanCode.DomainModels.Model
{
    /// <summary>A stronly typed id consisting of a guid prefixed with a lower-cased name of an entity.</summary>
    /// <remarks>Use <see cref="IdSlugAttribute"/> to override default entity prefix.</remarks>
    [TypedIdConverter]
    [SuppressMessage("?", "CA1000", Justification = "The methods are expected.")]
    [SuppressMessage("?", "CA1036", Justification = "We don't want to have easy comparison as it might be abused.")]
    public readonly struct SId<TEntity> : IEquatable<SId<TEntity>>, IComparable<SId<TEntity>>
        where TEntity : class, IIdentifiable<SId<TEntity>>
    {
        private const int GuidLength = 32;
        private const char Separator = '_';
        private static readonly string TypeName = SIdExtensions.GetPrefix<TEntity>();

        public static readonly int RawLength = GuidLength + 1 + TypeName.Length;
        public static readonly SId<TEntity> Empty = new(Guid.Empty);

        private readonly string? value;

        public string Value => value ?? Empty.Value;
        public bool IsEmpty => value is null || value == Empty;

        private SId(string v)
        {
            value = v;
        }

        public SId(Guid v)
        {
            value = string.Create(null, stackalloc char[RawLength], $"{TypeName}{Separator}{v:N}");
        }

        public static SId<TEntity> New() => new(Guid.NewGuid());

        [return: NotNullIfNotNull("id")]
        public static SId<TEntity>? FromNullable(string? id) => id is string v ? From(v) : (SId<TEntity>?)null;

        public static bool TryParse(string? v, out SId<TEntity> id)
        {
            if (IsValid(v))
            {
                id = new SId<TEntity>(v);
                return true;
            }
            else
            {
                id = default;
                return false;
            }
        }

        public static SId<TEntity> From(string v)
        {
            if (IsValid(v))
            {
                return new SId<TEntity>(v);
            }
            else
            {
                throw new FormatException($"The id has invalid format. It should look like {TypeName}{Separator}(random id).");
            }
        }

        public static SId<TEntity> FromGuidOrSId(string v)
        {
            if (TryParse(v, out var sid))
            {
                return sid;
            }
            else if (Guid.TryParse(v, out var guid))
            {
                return new(guid);
            }
            else
            {
                throw new FormatException($"The id cannot be parsed to SId nor Guid.");
            }
        }

        public static bool TryParseFromGuidOrSId([NotNullWhen(true)] string? v, out SId<TEntity> id)
        {
            if (IsValid(v))
            {
                id = new SId<TEntity>(v!);
                return true;
            }
            else if (Guid.TryParse(v, out var guid))
            {
                id = new(guid);
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
                    Guid.TryParseExact(span[(TypeName.Length + 1)..], "N", out _);
            }
        }

        public bool Equals(SId<TEntity> other) => Value.Equals(other.Value, StringComparison.Ordinal);
        public int CompareTo(SId<TEntity> other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is SId<TEntity> id && Equals(id);
        public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

        public static bool operator ==(SId<TEntity> left, SId<TEntity> right) => left.Equals(right);
        public static bool operator !=(SId<TEntity> left, SId<TEntity> right) => !left.Equals(right);

        public override string ToString() => Value;
        public static implicit operator string(SId<TEntity> id) => id.Value;
    }
}
