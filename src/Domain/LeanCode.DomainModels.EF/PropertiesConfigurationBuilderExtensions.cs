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
        switch (TId.Empty)
        {
            case IConstSizeTypedId:
            {
                var rawLength = (int)
                    typeof(TId).GetProperty(nameof(IConstSizeTypedId.RawLength))!.GetValue(null, null)!;
                return builder.HaveMaxLength(rawLength).AreFixedLength();
            }
            case IMaxLengthTypedId:
            {
                var maxLength = (int)
                    typeof(TId).GetProperty(nameof(IMaxLengthTypedId.MaxLength))!.GetValue(null, null)!;
                return builder.HaveMaxLength(maxLength);
            }
            default:
                return builder;
        }
    }

    private static PropertiesConfigurationBuilder<TId?> ConfigureFixedSizeOrMaxLengthIfPresent<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IPrefixedTypedId<TId>
    {
        switch (TId.Empty)
        {
            case IConstSizeTypedId:
            {
                var rawLength = (int)
                    typeof(TId).GetProperty(nameof(IConstSizeTypedId.RawLength))!.GetValue(null, null)!;
                return builder.HaveMaxLength(rawLength).AreFixedLength();
            }
            case IMaxLengthTypedId:
            {
                var maxLength = (int)
                    typeof(TId).GetProperty(nameof(IMaxLengthTypedId.MaxLength))!.GetValue(null, null)!;
                return builder.HaveMaxLength(maxLength);
            }
            default:
                return builder;
        }
    }

    private static PropertiesConfigurationBuilder<TId> ConfigureMaxLengthIfPresent<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IRawStringTypedId<TId>
    {
        if (TId.Empty is not IMaxLengthTypedId)
        {
            return builder;
        }

        var maxLength = (int)typeof(TId).GetProperty(nameof(IMaxLengthTypedId.MaxLength))!.GetValue(null, null)!;
        return builder.HaveMaxLength(maxLength);
    }

    private static PropertiesConfigurationBuilder<TId?> ConfigureMaxLengthIfPresent<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IRawStringTypedId<TId>
    {
        if (TId.Empty is not IMaxLengthTypedId)
        {
            return builder;
        }

        var maxLength = (int)typeof(TId).GetProperty(nameof(IMaxLengthTypedId.MaxLength))!.GetValue(null, null)!;
        return builder.HaveMaxLength(maxLength);
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
