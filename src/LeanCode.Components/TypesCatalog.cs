using System;
using System.Linq;
using System.Reflection;

namespace LeanCode.Components
{
    public sealed class TypesCatalog : IEquatable<TypesCatalog>
    {
        public Assembly[] Assemblies { get; }

        public TypesCatalog(params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }
            if (assemblies.Length == 0)
            {
                throw new ArgumentException("At least one assembly is required", nameof(assemblies));
            }
            Assemblies = assemblies;
        }

        public TypesCatalog(params Type[] types)
            : this(types.Select(t => t.GetTypeInfo().Assembly).ToArray())
        { }

        public Type GetType(string name)
        {
            return Assemblies.Select(a => a.GetType(name)).FirstOrDefault(t => t != null);
        }

        public bool Equals(TypesCatalog other)
        {
            if (other == null || Assemblies.Length != other.Assemblies.Length)
            {
                return false;
            }

            for (int i = 0; i < Assemblies.Length; i++)
            {
                if (!Assemblies[i].Equals(other.Assemblies[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(TypesCatalog) != obj.GetType())
            {
                return false;
            }

            return Equals((TypesCatalog)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int code = Assemblies[0].GetHashCode();
                for (int i = 1; i < Assemblies.Length; i++)
                {
                    code = ((code << 5) + code) ^ Assemblies[i].GetHashCode();
                }
                return code;
            }
        }
    }
}
