using System.Reflection;

namespace LeanCode.DomainModels.Model;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class IdSlugAttribute : Attribute
{
    public string Slug { get; }

    public IdSlugAttribute(string slug)
    {
        Slug = slug;
    }

    public static string? GetSlug<T>() => GetSlug(typeof(T));

    public static string? GetSlug(Type t) => t.GetCustomAttribute<IdSlugAttribute>()?.Slug;
}

internal static class SIdExtensions
{
    internal static string GetPrefix<TEntity>()
    {
        const int MaxTypeLength = 10;

        if (IdSlugAttribute.GetSlug<TEntity>() is string v)
        {
            return v;
        }
        else
        {
            var typename = typeof(TEntity).Name.ToLowerInvariant();

            if (typename.Length > MaxTypeLength)
            {
                return typename[0..MaxTypeLength];
            }
            else
            {
                return typename;
            }
        }
    }
}
