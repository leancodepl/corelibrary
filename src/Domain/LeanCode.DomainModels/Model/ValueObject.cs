using System;
using System.Collections.Generic;
using System.Linq;

namespace LeanCode.DomainModels.Model
{
    /// <summary>
    /// Utility class for implementing <see href="https://martinfowler.com/bliki/ValueObject.html">Value Object</see> pattern.
    /// <c>ValueObject</c> has structural equality semantics - two value objects are the same, when all of their properties are the same.
    /// </summary>
    /// <remarks>Value objects must be immutable</remarks>
    public abstract class ValueObject<T> where T : ValueObject<T>
    {
        /// <returns>Should return all of the value objects properties</returns>
        protected abstract object[] GetAttributesToIncludeInEqualityCheck();

        public override bool Equals(object other)
        {
            return Equals(other as T);
        }

        public bool Equals(T other)
        {
            if (other == null)
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
            return GenerateELFHash(GetAttributesToIncludeInEqualityCheck());
        }

        private int GenerateELFHash(object[] key)
        {
            int h = 0, g = 0;

            unchecked
            {
                const int c = (int)0xf0000000;
                for (int i = 0, len = key.Length; i < len; i++)
                {
                    h = (h << 4) + key[i].GetHashCode();

                    if ((g = h & c) != 0)
                        h ^= g >> 24;

                    h &= ~g;
                }
            }

            return (int)h;
        }
    }
}
