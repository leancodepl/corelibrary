using LeanCode.DomainModels.Ids;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeanCode.DomainModels.EF;

public interface ITypedIdStoreTypeProvider
{
    string PrefixedStoreType<TId>()
        where TId : struct, IPrefixedTypedId<TId>;

    string RawStringStoreType<TId>()
        where TId : struct, IRawStringTypedId<TId>;

    string RawStoreType<TId, TBacking>()
        where TBacking : struct, IEquatable<TBacking>, IComparable<TBacking>, ISpanParsable<TBacking>
        where TId : struct, IRawTypedId<TBacking, TId>;
}

public class PostgresTypedIdStoreTypeProvider : ITypedIdStoreTypeProvider
{
    public string PrefixedStoreType<TId>()
        where TId : struct, IPrefixedTypedId<TId> => "citext";

    public string RawStringStoreType<TId>()
        where TId : struct, IRawStringTypedId<TId> => "citext";

    public string RawStoreType<TId, TBacking>()
        where TId : struct, IRawTypedId<TBacking, TId>
        where TBacking : struct, IEquatable<TBacking>, IComparable<TBacking>, ISpanParsable<TBacking>
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

public class SqlServerTypedIdStoreTypeProvider : ITypedIdStoreTypeProvider
{
    public string PrefixedStoreType<TId>()
        where TId : struct, IPrefixedTypedId<TId> => $"nchar({TId.MaxLength})";

    public string RawStringStoreType<TId>()
        where TId : struct, IRawStringTypedId<TId> => $"nvarchar({TId.MaxLength})";

    public string RawStoreType<TId, TBacking>()
        where TId : struct, IRawTypedId<TBacking, TId>
        where TBacking : struct, IEquatable<TBacking>, IComparable<TBacking>, ISpanParsable<TBacking>
    {
        return typeof(TBacking) switch
        {
            Type t when t == typeof(int) => "int",
            Type t when t == typeof(long) => "bigint",
            Type t when t == typeof(Guid) => "uniqueidentifier",
            _ => throw new NotSupportedException(
                $"Backing type {typeof(TBacking).Name} is not supported for RawTypedId."
            ),
        };
    }
}

/// <summary>
/// Generic type mapping for PrefixedTypedId (PrefixedGuid and PrefixedUlid).
/// </summary>
public class PrefixedTypedIdTypeMapping<TId> : RelationalTypeMapping
    where TId : struct, IPrefixedTypedId<TId>
{
    private static readonly PrefixedTypedIdConverter<TId> ValueConverter = new();

    public PrefixedTypedIdTypeMapping(ITypedIdStoreTypeProvider storeTypeProvider)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(TId), ValueConverter),
                storeTypeProvider.PrefixedStoreType<TId>()
            )
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
/// </summary>
public class RawTypedIdTypeMapping<TBacking, TId> : RelationalTypeMapping
    where TBacking : struct, IEquatable<TBacking>, IComparable<TBacking>, ISpanParsable<TBacking>
    where TId : struct, IRawTypedId<TBacking, TId>
{
    private static readonly ValueConverter<TId, TBacking> ValueConverter = new(id => id.Value, TId.FromDatabase);

    public RawTypedIdTypeMapping(ITypedIdStoreTypeProvider storeTypeProvider)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(TId), ValueConverter),
                storeTypeProvider.RawStoreType<TId, TBacking>()
            )
        ) { }

    protected RawTypedIdTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
    {
        return new RawTypedIdTypeMapping<TBacking, TId>(parameters);
    }
}

/// <summary>
/// Generic type mapping for RawStringTypedId.
/// </summary>
public class RawStringTypedIdTypeMapping<TId> : RelationalTypeMapping
    where TId : struct, IRawStringTypedId<TId>
{
    private static readonly RawStringTypedIdConverter<TId> ValueConverter = new();

    public RawStringTypedIdTypeMapping(ITypedIdStoreTypeProvider storeTypeProvider)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(TId), ValueConverter),
                storeTypeProvider.RawStringStoreType<TId>()
            )
        ) { }

    protected RawStringTypedIdTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
    {
        return new RawStringTypedIdTypeMapping<TId>(parameters);
    }
}

public class TypedIdTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    private readonly ITypedIdStoreTypeProvider storeTypeProvider;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, RelationalTypeMapping?> cache = new();

    public TypedIdTypeMappingSourcePlugin(ITypedIdStoreTypeProvider storeTypeProvider)
    {
        this.storeTypeProvider = storeTypeProvider;
    }

    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var type = mappingInfo.ClrType;

        if (type == null)
        {
            return null;
        }

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
                    return (RelationalTypeMapping)Activator.CreateInstance(mappingType, storeTypeProvider)!;
                }
                else if (genericDefinition == typeof(IRawTypedId<,>) && iface.GetGenericArguments()[1] == type)
                {
                    var backingType = iface.GetGenericArguments()[0];
                    var mappingType = typeof(RawTypedIdTypeMapping<,>).MakeGenericType(backingType, type);
                    return (RelationalTypeMapping)Activator.CreateInstance(mappingType, storeTypeProvider)!;
                }
                else if (genericDefinition == typeof(IRawStringTypedId<>) && iface.GetGenericArguments()[0] == type)
                {
                    var mappingType = typeof(RawStringTypedIdTypeMapping<>).MakeGenericType(type);
                    return (RelationalTypeMapping)Activator.CreateInstance(mappingType, storeTypeProvider)!;
                }
            }
        }

        return null;
    }
}
