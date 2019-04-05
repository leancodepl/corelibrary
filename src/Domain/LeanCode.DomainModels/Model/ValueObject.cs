using System;
using System.Collections.Generic;
using System.Linq;

namespace LeanCode.DomainModels.Model
{
    public abstract class ValueObject<T> where T : ValueObject<T>
    {
        protected abstract object[] GetAttributesToIncludeInEqualityCheck();

        public override bool Equals(object other)
        {
            return Equals(other as T);
        }

        public bool Equals(T other)
        {
            if (other is null)
                return false;

            return GetAttributesToIncludeInEqualityCheck().SequenceEqual(other.GetAttributesToIncludeInEqualityCheck());
        }

        public static bool operator ==(ValueObject<T> left, ValueObject<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            var attrs = GetAttributesToIncludeInEqualityCheck();
            var hc = new HashCode();
            for (int i = 0; i < attrs.Length; i++)
            {
                hc.Add(attrs[i]);
            }
            return hc.ToHashCode();
        }
    }
}
