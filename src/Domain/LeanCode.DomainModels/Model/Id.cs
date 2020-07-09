using System;
using System.Diagnostics;
using LeanCode.DomainModels.Serialization;
using LeanCode.IdentityProvider;

namespace LeanCode.DomainModels.Model
{
    [DebuggerDisplay("{Value}")]
    [TypedIdConverter]
    public readonly struct Id<TEntity> : IEquatable<Id<TEntity>>, IComparable<Id<TEntity>>
        where TEntity : class, IIdentifiable<Id<TEntity>>
    {
        public Guid Value { get; }

        public Id(Guid value)
        {
            Value = value;
        }

        public static Id<TEntity> New() => new Id<TEntity>(Identity.NewId());
        public static Id<TEntity> From(Guid id) => new Id<TEntity>(id);
        public static Id<TEntity>? From(Guid? id) => id is Guid v ? new Id<TEntity>(v) : (Id<TEntity>?)null;

        public bool Equals(Id<TEntity> other) => Value.Equals(other.Value);
        public int CompareTo(Id<TEntity> other) => Value.CompareTo(other.Value);
        public override bool Equals(object? obj) => obj is Id<TEntity> id && Value.Equals(id.Value);
        public override int GetHashCode() => HashCode.Combine(Value);
        public override string? ToString() => Value.ToString();

        public static bool operator ==(Id<TEntity> left, Id<TEntity> right) => left.Equals(right);
        public static bool operator !=(Id<TEntity> left, Id<TEntity> right) => !left.Equals(right);
        public static implicit operator Guid(Id<TEntity> id) => id.Value;
        public static explicit operator Id<TEntity>(Guid id) => new Id<TEntity>(id);
    }

    [DebuggerDisplay("{Value}")]
    [TypedIdConverter]
    public readonly struct IId<TEntity> : IEquatable<IId<TEntity>>, IComparable<IId<TEntity>>
        where TEntity : class, IIdentifiable<IId<TEntity>>
    {
        public static readonly IId<TEntity> EmptyValue = IId<TEntity>.From(0);

        public int Value { get; }

        public IId(int value)
        {
            Value = value;
        }

        public static IId<TEntity> From(int id) => new IId<TEntity>(id);
        public static IId<TEntity>? From(int? id) => id is int v ? new IId<TEntity>(v) : (IId<TEntity>?)null;

        public bool Equals(IId<TEntity> other) => Value.Equals(other.Value);
        public int CompareTo(IId<TEntity> other) => Value.CompareTo(other.Value);
        public override bool Equals(object? obj) => obj is IId<TEntity> id && Value.Equals(id.Value);
        public override int GetHashCode() => HashCode.Combine(Value);
        public override string? ToString() => Value.ToString();

        public static bool operator ==(IId<TEntity> left, IId<TEntity> right) => left.Equals(right);
        public static bool operator !=(IId<TEntity> left, IId<TEntity> right) => !left.Equals(right);
        public static implicit operator int(IId<TEntity> id) => id.Value;
        public static explicit operator IId<TEntity>(int id) => new IId<TEntity>(id);
    }
}
