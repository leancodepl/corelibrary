using LeanCode.DomainModels.Ids;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeanCode.DomainModels.EF;

/// <summary>
/// Generic type mapping for PrefixedTypedId (PrefixedGuid and PrefixedUlid).
/// Maps to PostgreSQL citext type.
/// </summary>
public class PrefixedTypedIdTypeMapping<TId> : RelationalTypeMapping
    where TId : struct, IPrefixedTypedId<TId>
{
    private static readonly PrefixedTypedIdConverter<TId> ValueConverter = new();

    public PrefixedTypedIdTypeMapping()
        : base(
            new RelationalTypeMappingParameters(new CoreTypeMappingParameters(typeof(TId), ValueConverter), "citext")
        ) { }

    protected PrefixedTypedIdTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
    {
        return new PrefixedTypedIdTypeMapping<TId>(parameters);
    }
}

/// <summary>
/// Generic type mapping for RawTypedId with any backing type.
/// Automatically determines the correct PostgreSQL type based on the backing type.
/// </summary>
public class RawTypedIdTypeMapping<TBacking, TId> : RelationalTypeMapping
    where TBacking : struct, IEquatable<TBacking>, IComparable<TBacking>, ISpanParsable<TBacking>
    where TId : struct, IRawTypedId<TBacking, TId>
{
    private static readonly ValueConverter<TId, TBacking> ValueConverter = new(id => id.Value, TId.FromDatabase);
    private static readonly ValueComparer<TId> ValueComparer = new(TId.DatabaseEquals, d => d.GetHashCode());

    public RawTypedIdTypeMapping()
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(TId), ValueConverter, ValueComparer),
                GetStoreType()
            )
        ) { }

    protected RawTypedIdTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
    {
        return new RawTypedIdTypeMapping<TBacking, TId>(parameters);
    }

    private static string GetStoreType()
    {
        return typeof(TBacking) switch
        {
            Type t when t == typeof(int) => "integer",
            Type t when t == typeof(long) => "bigint",
            Type t when t == typeof(Guid) => "uuid",
            _ => throw new NotSupportedException(
                $"Backing type {typeof(TBacking).Name} is not supported for RawTypedId."
            ),
        };
    }
}

public class TypedIdTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var type = mappingInfo.ClrType;

        if (type == null)
        {
            return null;
        }

        return CreateMapping(type);
    }

    private static RelationalTypeMapping? CreateMapping(Type type)
    {
        if (type.IsAbstract || !type.IsValueType)
        {
            return null;
        }

        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType)
            {
                var genericDefinition = iface.GetGenericTypeDefinition();
                if (genericDefinition == typeof(IPrefixedTypedId<>) && iface.GetGenericArguments()[0] == type)
                {
                    var mappingType = typeof(PrefixedTypedIdTypeMapping<>).MakeGenericType(type);
                    return (RelationalTypeMapping)Activator.CreateInstance(mappingType)!;
                }
                else if (genericDefinition == typeof(IRawTypedId<,>) && iface.GetGenericArguments()[1] == type)
                {
                    var backingType = iface.GetGenericArguments()[0];
                    var mappingType = typeof(RawTypedIdTypeMapping<,>).MakeGenericType(backingType, type);
                    return (RelationalTypeMapping)Activator.CreateInstance(mappingType)!;
                }
            }
        }

        return null;
    }
}
