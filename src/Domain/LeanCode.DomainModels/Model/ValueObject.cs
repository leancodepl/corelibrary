using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LeanCode.DomainModels.Model
{
    public abstract class ValueObject<T> : IEquatable<T>
        where T : notnull, ValueObject<T>
    {
        protected abstract object?[] GetAttributesToIncludeInEqualityCheck();

        public override int GetHashCode()
        {
            var hc = new HashCode();

            foreach (object? attr in GetAttributesToIncludeInEqualityCheck())
            {
                hc.Add(attr);
            }

            return hc.ToHashCode();
        }

        public override bool Equals(object? other) => Equals(other as T);

        public bool Equals([AllowNull] T other)
        {
            if (other is null)
            {
                return false;
            }

            return GetAttributesToIncludeInEqualityCheck()
                .SequenceEqual(other.GetAttributesToIncludeInEqualityCheck());
        }

        public static bool operator ==(ValueObject<T> left, ValueObject<T> right) => Equals(left, right);

        public static bool operator !=(ValueObject<T> left, ValueObject<T> right) => !Equals(left, right);
    }
}
