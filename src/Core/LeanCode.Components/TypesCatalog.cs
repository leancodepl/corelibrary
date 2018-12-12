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
            if (assemblies is null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }
            Assemblies = assemblies;
        }

        public TypesCatalog(params Type[] types)
        {
            if (types is null)
            {
                throw new ArgumentNullException(nameof(types));
            }
            Assemblies = types
                .Select(t => t.Assembly)
                .Distinct()
                .ToArray();
        }

        public static TypesCatalog Of<T1>() => new TypesCatalog(typeof(T1));
        public static TypesCatalog Of<T1, T2>() => new TypesCatalog(typeof(T1), typeof(T2));
        public static TypesCatalog Of<T1, T2, T3>() => new TypesCatalog(typeof(T1), typeof(T2), typeof(T3));

        public Type GetType(string name)
        {
            return Assemblies
                .Select(a => a.GetType(name))
                .FirstOrDefault(t => t != null);
        }

        public bool Equals(TypesCatalog other)
        {
            if (other is null || Assemblies.Length != other.Assemblies.Length)
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
            if (obj is null || typeof(TypesCatalog) != obj.GetType())
            {
                return false;
            }

            return Equals((TypesCatalog)obj);
        }

        public override int GetHashCode()
        {
            var hc = new HashCode();
            for (int i = 0; i < Assemblies.Length; i++)
            {
                hc.Add(Assemblies[i]);
            }
            return hc.ToHashCode();
        }

        public TypesCatalog Merge(TypesCatalog other)
        {
            var assemblies = this.Assemblies.ToList();

            foreach (var assembly in other.Assemblies)
            {
                if (!assemblies.Any(assembly.Equals))
                {
                    assemblies.Add(assembly);
                }
            }

            return new TypesCatalog(assemblies.ToArray());
        }
    }
}
