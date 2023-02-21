using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace LeanCode.Components;

public sealed class TypesCatalog : IEquatable<TypesCatalog>
{
    public IReadOnlyList<Assembly> Assemblies { get; }

    public TypesCatalog(params Assembly[] assemblies)
    {
        Assemblies = assemblies
            .Distinct()
            .ToArray();
    }

    public TypesCatalog(params Type[] types)
    {
        Assemblies = types
            .Select(t => t.Assembly)
            .Distinct()
            .ToArray();
    }

    public static TypesCatalog Of<T1>() => new TypesCatalog(typeof(T1));
    public static TypesCatalog Of<T1, T2>() => new TypesCatalog(typeof(T1), typeof(T2));
    public static TypesCatalog Of<T1, T2, T3>() => new TypesCatalog(typeof(T1), typeof(T2), typeof(T3));

    public Type? GetType(string name)
    {
        return Assemblies
            .Select(a => a.GetType(name))
            .FirstOrDefault(t => t != null);
    }

    public bool Equals([AllowNull] TypesCatalog other)
    {
        if (other is null || Assemblies.Count != other.Assemblies.Count)
        {
            return false;
        }

        return Assemblies.SequenceEqual(other.Assemblies);
    }

    public override bool Equals(object? obj) => Equals(obj as TypesCatalog);

    public override int GetHashCode()
    {
        var hc = new HashCode();

        foreach (var assembly in Assemblies)
        {
            hc.Add(assembly);
        }

        return hc.ToHashCode();
    }

    public TypesCatalog Merge(TypesCatalog other) =>
        new TypesCatalog(Assemblies.Union(other.Assemblies).ToArray());
}
