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
            .ConfigureMaxLengthIfPresent();
    }

    public static PropertiesConfigurationBuilder<TId?> AreStringTypedId<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IRawStringTypedId<TId>
    {
        return builder
            .HaveConversion<RawStringTypedIdConverter<TId>, RawStringTypedIdComparer<TId>>()
            .ConfigureMaxLengthIfPresent();
    }

    public static PropertiesConfigurationBuilder<TId> ArePrefixedTypedId<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IPrefixedTypedId<TId>
    {
        return builder
            .HaveConversion<PrefixedTypedIdConverter<TId>, PrefixedTypedIdComparer<TId>>()
            .ConfigureFixedSizeOrMaxLengthIfPresent();
    }

    public static PropertiesConfigurationBuilder<TId?> ArePrefixedTypedId<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IPrefixedTypedId<TId>
    {
        return builder
            .HaveConversion<PrefixedTypedIdConverter<TId>, PrefixedTypedIdComparer<TId>>()
            .ConfigureFixedSizeOrMaxLengthIfPresent();
    }

    private static PropertiesConfigurationBuilder<TId> ConfigureFixedSizeOrMaxLengthIfPresent<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IPrefixedTypedId<TId>
    {
        if (IHasEmptyId<TId>.GetRawLength() is { } rawLength)
        {
            return builder.HaveMaxLength(rawLength).AreFixedLength();
        }
        else if (IHasEmptyId<TId>.GetMaxLength() is { } maxLength)
        {
            return builder.HaveMaxLength(maxLength);
        }
        else
        {
            return builder;
        }
    }

    private static PropertiesConfigurationBuilder<TId?> ConfigureFixedSizeOrMaxLengthIfPresent<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IHasEmptyId<TId>
    {
        if (IHasEmptyId<TId>.GetRawLength() is { } rawLength)
        {
            return builder.HaveMaxLength(rawLength).AreFixedLength();
        }
        else if (IHasEmptyId<TId>.GetMaxLength() is { } maxLength)
        {
            return builder.HaveMaxLength(maxLength);
        }
        else
        {
            return builder;
        }
    }

    private static PropertiesConfigurationBuilder<TId> ConfigureMaxLengthIfPresent<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IHasEmptyId<TId>
    {
        return IHasEmptyId<TId>.GetMaxLength() is { } maxLength ? builder.HaveMaxLength(maxLength) : builder;
    }

    private static PropertiesConfigurationBuilder<TId?> ConfigureMaxLengthIfPresent<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IHasEmptyId<TId>
    {
        return IHasEmptyId<TId>.GetMaxLength() is { } maxLength ? builder.HaveMaxLength(maxLength) : builder;
    }

    private static PropertiesConfigurationBuilder<TId> AreRawTypedId<TBacking, TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TBacking : struct
        where TId : struct, IRawTypedId<TBacking, TId>
    {
        return builder.HaveConversion<RawTypedIdConverter<TBacking, TId>, RawTypedIdComparer<TBacking, TId>>();
    }

    private static PropertiesConfigurationBuilder<TId?> AreRawTypedId<TBacking, TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TBacking : struct
        where TId : struct, IRawTypedId<TBacking, TId>
    {
        return builder.HaveConversion<RawTypedIdConverter<TBacking, TId>, RawTypedIdComparer<TBacking, TId>>();
    }
}
