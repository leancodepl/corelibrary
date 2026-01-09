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

public class PrefixedTypedIdTypeMappingPlugin<TId> : IRelationalTypeMappingSourcePlugin
    where TId : struct, IPrefixedTypedId<TId>
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        if (mappingInfo.ClrType == typeof(TId))
        {
            return new PrefixedTypedIdTypeMapping<TId>();
        }

        return null;
    }
}

public class RawTypedIdTypeMappingPlugin<TBacking, TId> : IRelationalTypeMappingSourcePlugin
    where TBacking : struct, IEquatable<TBacking>, IComparable<TBacking>, ISpanParsable<TBacking>
    where TId : struct, IRawTypedId<TBacking, TId>
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        if (mappingInfo.ClrType == typeof(TId))
        {
            return new RawTypedIdTypeMapping<TBacking, TId>();
        }

        return null;
    }
}
