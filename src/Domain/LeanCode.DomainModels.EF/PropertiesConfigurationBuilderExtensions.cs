using LeanCode.DomainModels.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanCode.DomainModels.EF;

public static class PropertiesConfigurationBuilderExtensions
{
    public static PropertiesConfigurationBuilder<TId> AreTypedId<TBacking, TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TBacking : struct
        where TId : struct, IRawTypedId<TBacking, TId>
    {
        return builder.HaveConversion<RawTypedIdConverter<TBacking, TId>>();
    }

    public static PropertiesConfigurationBuilder<TId?> AreTypedId<TBacking, TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TBacking : struct
        where TId : struct, IRawTypedId<TBacking, TId>
    {
        return builder.HaveConversion<RawTypedIdConverter<TBacking, TId>>();
    }

    public static PropertiesConfigurationBuilder<TId> AreTypedId<TId>(this PropertiesConfigurationBuilder<TId> builder)
        where TId : struct, IPrefixedTypedId<TId>
    {
        return builder.HaveConversion<PrefixedTypedIdConverter<TId>>().HaveMaxLength(TId.RawLength).AreFixedLength();
    }

    public static PropertiesConfigurationBuilder<TId?> AreTypedId<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IPrefixedTypedId<TId>
    {
        return builder.HaveConversion<PrefixedTypedIdConverter<TId>>().HaveMaxLength(TId.RawLength).AreFixedLength();
    }
}
