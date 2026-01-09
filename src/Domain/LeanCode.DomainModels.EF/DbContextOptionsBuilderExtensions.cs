using System.Reflection;
using LeanCode.DomainModels.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.EF;

public static class DbContextOptionsBuilderExtensions
{
    private static readonly MethodInfo AddPrefixedTypedIdPluginMethod =
        typeof(DbContextOptionsBuilderExtensions).GetMethod(nameof(AddPrefixedTypedIdPlugin))!;

    private static readonly MethodInfo AddRawTypedIdPluginMethod = typeof(DbContextOptionsBuilderExtensions).GetMethod(
        nameof(AddRawTypedIdPlugin)
    )!;

    /// <summary>
    /// Automatically registers TypeMappingPlugins for all TypedIds found in the specified assemblies.
    /// Scans for types implementing IPrefixedTypedId and IRawTypedId with any backing type (int, long, Guid).
    /// </summary>
    public static DbContextOptionsBuilder AddAllTypedIdPlugins(
        this DbContextOptionsBuilder builder,
        params Assembly[] assemblies
    )
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                if (IsPrefixedTypedId(type))
                {
                    var method = AddPrefixedTypedIdPluginMethod.MakeGenericMethod(type);
                    method.Invoke(null, [builder]);
                }
                else if (TryGetRawTypedIdBackingType(type, out var backingType))
                {
                    var method = AddRawTypedIdPluginMethod.MakeGenericMethod(backingType, type);
                    method.Invoke(null, [builder]);
                }
            }
        }

        return builder;
    }

    public static DbContextOptionsBuilder AddPrefixedTypedIdPlugin<TId>(this DbContextOptionsBuilder builder)
        where TId : struct, IPrefixedTypedId<TId>
    {
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(
            new PrefixedTypedIdDbContextOptionsExtension<TId>()
        );

        return builder;
    }

    public static DbContextOptionsBuilder AddRawTypedIdPlugin<TBacking, TId>(this DbContextOptionsBuilder builder)
        where TBacking : struct, IEquatable<TBacking>, IComparable<TBacking>, ISpanParsable<TBacking>
        where TId : struct, IRawTypedId<TBacking, TId>
    {
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(
            new RawTypedIdDbContextOptionsExtension<TBacking, TId>()
        );

        return builder;
    }

    private static bool IsPrefixedTypedId(Type type)
    {
        return type.GetInterfaces()
            .Any(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IPrefixedTypedId<>)
                && i.GetGenericArguments()[0] == type
            );
    }

    private static bool TryGetRawTypedIdBackingType(Type type, out Type backingType)
    {
        var rawTypedIdInterface = type.GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IRawTypedId<,>)
                && i.GetGenericArguments()[1] == type
            );

        if (rawTypedIdInterface != null)
        {
            backingType = rawTypedIdInterface.GetGenericArguments()[0];
            return true;
        }

        backingType = null!;
        return false;
    }
}

internal class PrefixedTypedIdDbContextOptionsExtension<TId> : IDbContextOptionsExtension
    where TId : struct, IPrefixedTypedId<TId>
{
    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton<IRelationalTypeMappingSourcePlugin, PrefixedTypedIdTypeMappingPlugin<TId>>();
    }

    public void Validate(IDbContextOptions options) { }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;

        public override string LogFragment => $"PrefixedTypedIdPlugin<{typeof(TId).Name}>";

        public override int GetServiceProviderHashCode() => typeof(TId).GetHashCode();

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return other is ExtensionInfo otherExtension && otherExtension.Extension.GetType() == Extension.GetType();
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo[$"PrefixedTypedId:{typeof(TId).Name}"] = "1";
        }
    }
}

internal class RawTypedIdDbContextOptionsExtension<TBacking, TId> : IDbContextOptionsExtension
    where TBacking : struct, IEquatable<TBacking>, IComparable<TBacking>, ISpanParsable<TBacking>
    where TId : struct, IRawTypedId<TBacking, TId>
{
    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton<IRelationalTypeMappingSourcePlugin, RawTypedIdTypeMappingPlugin<TBacking, TId>>();
    }

    public void Validate(IDbContextOptions options) { }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;

        public override string LogFragment => $"RawTypedIdPlugin<{typeof(TBacking).Name}, {typeof(TId).Name}>";

        public override int GetServiceProviderHashCode() => HashCode.Combine(typeof(TBacking), typeof(TId));

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return other is ExtensionInfo otherExtension && otherExtension.Extension.GetType() == Extension.GetType();
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo[$"RawTypedId<{typeof(TBacking).Name}>:{typeof(TId).Name}"] = "1";
        }
    }
}
