using System;
using System.Diagnostics;
using LeanCode.DomainModels.Serialization;

namespace LeanCode.DomainModels.Model
{
    [DebuggerDisplay("{Value}")]
    [TypedIdConverter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1000", Justification = "The methods are expected.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1036", Justification = "We don't want to have easy comparison as it might be abused.")]
    public readonly struct Id<TEntity> : IEquatable<Id<TEntity>>, IComparable<Id<TEntity>>
        where TEntity : class, IIdentifiable<Id<TEntity>>
    {
        public static readonly Id<TEntity> Empty;

        public Guid Value { get; }

        public Id(Guid value)
        {
            Value = value;
        }

        public static Id<TEntity> New() => new(Guid.NewGuid());
        public static Id<TEntity> From(Guid id) => new(id);
        public static Id<TEntity>? From(Guid? id) => id is Guid v ? new Id<TEntity>(v) : (Id<TEntity>?)null;

        public bool Equals(Id<TEntity> other) => Value.Equals(other.Value);
        public int CompareTo(Id<TEntity> other) => Value.CompareTo(other.Value);
        public override bool Equals(object? obj) => obj is Id<TEntity> id && Value.Equals(id.Value);
        public override int GetHashCode() => HashCode.Combine(Value);
        public override string? ToString() => Value.ToString();

        public static bool operator ==(Id<TEntity> left, Id<TEntity> right) => left.Equals(right);
        public static bool operator !=(Id<TEntity> left, Id<TEntity> right) => !left.Equals(right);

        public Guid ToGuid() => Value;
        public static implicit operator Guid(Id<TEntity> id) => id.Value;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2225", Justification = "Already provided as `From`.")]
        public static explicit operator Id<TEntity>(Guid id) => new(id);
    }

    [DebuggerDisplay("{Value}")]
    [TypedIdConverter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1000", Justification = "The methods are expected.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1036", Justification = "We don't want to have easy comparison as it might be abused.")]
    public readonly struct IId<TEntity> : IEquatable<IId<TEntity>>, IComparable<IId<TEntity>>
        where TEntity : class, IIdentifiable<IId<TEntity>>
    {
        public static readonly IId<TEntity> Empty;

        public int Value { get; }

        public IId(int value)
        {
            Value = value;
        }

        public static IId<TEntity> From(int id) => new(id);
        public static IId<TEntity>? From(int? id) => id is int v ? new IId<TEntity>(v) : (IId<TEntity>?)null;

        public bool Equals(IId<TEntity> other) => Value.Equals(other.Value);
        public int CompareTo(IId<TEntity> other) => Value.CompareTo(other.Value);
        public override bool Equals(object? obj) => obj is IId<TEntity> id && Value.Equals(id.Value);
        public override int GetHashCode() => HashCode.Combine(Value);
        public override string? ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

        public static bool operator ==(IId<TEntity> left, IId<TEntity> right) => left.Equals(right);
        public static bool operator !=(IId<TEntity> left, IId<TEntity> right) => !left.Equals(right);

        public int ToInt32() => Value;
        public static implicit operator int(IId<TEntity> id) => id.Value;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2225", Justification = "Already provided as `From`.")]
        public static explicit operator IId<TEntity>(int id) => new(id);
    }

    [DebuggerDisplay("{Value}")]
    [TypedIdConverter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1000", Justification = "The methods are expected.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1036", Justification = "We don't want to have easy comparison as it might be abused.")]
    public readonly struct LId<TEntity> : IEquatable<LId<TEntity>>, IComparable<LId<TEntity>>
        where TEntity : class, IIdentifiable<LId<TEntity>>
    {
        public static readonly LId<TEntity> Empty = LId<TEntity>.From(0);

        public long Value { get; }

        public LId(long value)
        {
            Value = value;
        }

        public static LId<TEntity> From(long id) => new(id);
        public static LId<TEntity>? From(long? id) => id is long v ? new LId<TEntity>(v) : (LId<TEntity>?)null;

        public bool Equals(LId<TEntity> other) => Value.Equals(other.Value);
        public int CompareTo(LId<TEntity> other) => Value.CompareTo(other.Value);
        public override bool Equals(object? obj) => obj is LId<TEntity> id && Value.Equals(id.Value);
        public override int GetHashCode() => HashCode.Combine(Value);
        public override string? ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public static bool operator ==(LId<TEntity> left, LId<TEntity> right) => left.Equals(right);
        public static bool operator !=(LId<TEntity> left, LId<TEntity> right) => !left.Equals(right);

        public long ToInt64() => Value;
        public static implicit operator long(LId<TEntity> id) => id.Value;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2225", Justification = "Already provided as `From`.")]
        public static explicit operator LId<TEntity>(long id) => new(id);
    }
}
