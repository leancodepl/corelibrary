using System.Reflection;
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

    public static PropertiesConfigurationBuilder<TId> ArePrefixedTypedId<TId>(
        this PropertiesConfigurationBuilder<TId> builder
    )
        where TId : struct, IPrefixedTypedId<TId>
    {
        builder = builder.HaveConversion<PrefixedTypedIdConverter<TId>, PrefixedTypedIdComparer<TId>>();

        if (TId.Empty is IConstSizeTypedId)
        {
            var rawLength = (int)typeof(TId).GetProperty(nameof(IConstSizeTypedId.RawLength))!.GetValue(null, null)!;
            builder.HaveMaxLength(rawLength).AreFixedLength();
        }
        else if (TId.Empty is IMaxLengthTypedId)
        {
            var maxLength = (int)typeof(TId).GetProperty(nameof(IMaxLengthTypedId.MaxLength))!.GetValue(null, null)!;
            builder.HaveMaxLength(maxLength);
        }

        return builder;
    }

    public static PropertiesConfigurationBuilder<TId?> ArePrefixedTypedId<TId>(
        this PropertiesConfigurationBuilder<TId?> builder
    )
        where TId : struct, IPrefixedTypedId<TId>
    {
        builder = builder.HaveConversion<PrefixedTypedIdConverter<TId>, PrefixedTypedIdComparer<TId>>();

        if (TId.Empty is IConstSizeTypedId)
        {
            var rawLength = (int)typeof(TId).GetProperty(nameof(IConstSizeTypedId.RawLength))!.GetValue(null, null)!;
            builder.HaveMaxLength(rawLength).AreFixedLength();
        }
        else if (TId.Empty is IMaxLengthTypedId)
        {
            var maxLength = (int)typeof(TId).GetProperty(nameof(IMaxLengthTypedId.MaxLength))!.GetValue(null, null)!;
            builder.HaveMaxLength(maxLength);
        }

        return builder;
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
