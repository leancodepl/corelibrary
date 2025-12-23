using LeanCode.DomainModels.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanCode.DomainModels.EF;

public static class PropertiesConfigurationBuilderExtensions
{
    public static PropertiesConfigurationBuilder<TId> AreIntTypedId<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IRawTypedId<int, TId>
    {
        return builder.AreRawTypedId<int, TId>();
    }

    public static PropertiesConfigurationBuilder<TId?> AreIntTypedId<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IRawTypedId<int, TId>
    {
        return builder.AreRawTypedId<int, TId>();
    }

    public static PropertiesConfigurationBuilder<TId> AreLongTypedId<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IRawTypedId<long, TId>
    {
        return builder.AreRawTypedId<long, TId>();
    }

    public static PropertiesConfigurationBuilder<TId?> AreLongTypedId<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IRawTypedId<long, TId>
    {
        return builder.AreRawTypedId<long, TId>();
    }

    public static PropertiesConfigurationBuilder<TId> AreGuidTypedId<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IRawTypedId<Guid, TId>
    {
        return builder.AreRawTypedId<Guid, TId>();
    }

    public static PropertiesConfigurationBuilder<TId?> AreGuidTypedId<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IRawTypedId<Guid, TId>
    {
        return builder.AreRawTypedId<Guid, TId>();
    }

    public static PropertiesConfigurationBuilder<TId> AreStringTypedId<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IRawStringTypedId<TId>
    {
        return builder
            .HaveConversion<RawStringTypedIdConverter<TId>, RawStringTypedIdComparer<TId>>()
            .ConfigureMaxLength();
    }

    public static PropertiesConfigurationBuilder<TId?> AreStringTypedId<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IRawStringTypedId<TId>
    {
        return builder
            .HaveConversion<RawStringTypedIdConverter<TId>, RawStringTypedIdComparer<TId>>()
            .ConfigureMaxLength();
    }

    public static PropertiesConfigurationBuilder<TId> ArePrefixedTypedId<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IPrefixedTypedId<TId>
    {
        return builder
            .HaveConversion<PrefixedTypedIdConverter<TId>, PrefixedTypedIdComparer<TId>>()
            .ConfigureMaxLength();
    }

    public static PropertiesConfigurationBuilder<TId?> ArePrefixedTypedId<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IPrefixedTypedId<TId>
    {
        return builder
            .HaveConversion<PrefixedTypedIdConverter<TId>, PrefixedTypedIdComparer<TId>>()
            .ConfigureMaxLength();
    }

    private static PropertiesConfigurationBuilder<TId> ConfigureMaxLength<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IMaxLengthTypedId
    {
        return builder.HaveMaxLength(TId.MaxLength).AreFixedLength();
    }

    private static PropertiesConfigurationBuilder<TId?> ConfigureMaxLength<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IMaxLengthTypedId
    {
        return builder.HaveMaxLength(TId.MaxLength).AreFixedLength();
    }

    private static PropertiesConfigurationBuilder<TId> AreRawTypedId<TBacking, TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TBacking : struct, IEquatable<TBacking>, IComparable<TBacking>, ISpanParsable<TBacking>
        where TId : struct, IRawTypedId<TBacking, TId>
    {
        return builder.HaveConversion<RawTypedIdConverter<TBacking, TId>, RawTypedIdComparer<TBacking, TId>>();
    }

    private static PropertiesConfigurationBuilder<TId?> AreRawTypedId<TBacking, TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TBacking : struct, IEquatable<TBacking>, IComparable<TBacking>, ISpanParsable<TBacking>
        where TId : struct, IRawTypedId<TBacking, TId>
    {
        return builder.HaveConversion<RawTypedIdConverter<TBacking, TId>, RawTypedIdComparer<TBacking, TId>>();
    }
}
