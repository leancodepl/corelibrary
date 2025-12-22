using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

[SuppressMessage("?", "EF1001", Justification = "Tests.")]
public class TypedIdConverterTests
{
    [Fact]
    public void RawInt_conversion_to_int_and_back_works()
    {
        AssertConvertsInt(new(-1));
        AssertConvertsInt(new(0));
        AssertConvertsInt(new(1));
    }

    [Fact]
    public void RawLong_conversion_to_long_and_back_works()
    {
        AssertConvertsLong(new(-1));
        AssertConvertsLong(new(0));
        AssertConvertsLong(new(1));
    }

    [Fact]
    public void RawGuid_conversion_to_guid_and_back_works()
    {
        AssertConvertsGuid(new(Guid.Empty));
        AssertConvertsGuid(new(Guid.NewGuid()));
    }

    [Fact]
    public void RawString_conversion_to_string_and_back_works()
    {
        AssertConvertsRawString(new(""));
        AssertConvertsRawString(new("test-id"));
        AssertConvertsRawString(new("another-value"));
    }

    [Fact]
    public void PrefixedGuid_conversion_to_guid_and_back_works()
    {
        AssertConvertsPrefixedGuid(new(Guid.Empty));
        AssertConvertsPrefixedGuid(new(Guid.NewGuid()));
        AssertConvertsPrefixedGuid(PrefixedGuidId.New());
    }

    [Fact]
    public void PrefixedString_conversion_to_string_and_back_works()
    {
        AssertConvertsPrefixedString(PrefixedStringId.FromValuePart(""));
        AssertConvertsPrefixedString(PrefixedStringId.FromValuePart("test-id"));
        AssertConvertsPrefixedString(PrefixedStringId.FromValuePart("another-value"));
    }

    [Fact]
    public void RawInt_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<IntId>().AreIntTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(IntId));
        Assert.NotNull(mapping);
        Assert.IsType<RawTypedIdConverter<int, IntId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(RawTypedIdComparer<int, IntId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(IntId), mapping.ClrType);
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void RawLong_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<LongId>().AreLongTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(LongId));
        Assert.NotNull(mapping);
        Assert.IsType<RawTypedIdConverter<long, LongId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(RawTypedIdComparer<long, LongId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(LongId), mapping.ClrType);
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void RawGuid_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<GuidId>().AreGuidTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(GuidId));
        Assert.NotNull(mapping);
        Assert.IsType<RawTypedIdConverter<Guid, GuidId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(RawTypedIdComparer<Guid, GuidId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(GuidId), mapping.ClrType);
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void RawString_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<StringId>().AreStringTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(StringId));
        Assert.NotNull(mapping);
        Assert.IsType<RawStringTypedIdConverter<StringId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(RawStringTypedIdComparer<StringId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(StringId), mapping.ClrType);
        Assert.Equal(StringId.MaxLength, mapping.GetMaxLength());
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void PrefixedGuid_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<PrefixedGuidId>().ArePrefixedTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(PrefixedGuidId));
        Assert.NotNull(mapping);
        Assert.IsType<PrefixedTypedIdConverter<PrefixedGuidId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(PrefixedTypedIdComparer<PrefixedGuidId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(PrefixedGuidId), mapping.ClrType);
        Assert.Equal(PrefixedGuidId.MaxLength, mapping.GetMaxLength());
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void PrefixedString_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<PrefixedStringId>().ArePrefixedTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(PrefixedStringId));
        Assert.NotNull(mapping);
        Assert.IsType<PrefixedTypedIdConverter<PrefixedStringId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(PrefixedTypedIdComparer<PrefixedStringId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(PrefixedStringId), mapping.ClrType);
        Assert.Equal(PrefixedStringId.MaxLength, mapping.GetMaxLength());
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void OptionalRawInt_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<IntId?>().AreIntTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(IntId?));
        Assert.NotNull(mapping);
        Assert.IsType<RawTypedIdConverter<int, IntId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(RawTypedIdComparer<int, IntId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(IntId?), mapping.ClrType);
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void OptionalRawLong_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<LongId?>().AreLongTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(LongId?));
        Assert.NotNull(mapping);
        Assert.IsType<RawTypedIdConverter<long, LongId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(RawTypedIdComparer<long, LongId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(LongId?), mapping.ClrType);
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void OptionalRawGuid_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<GuidId?>().AreGuidTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(GuidId?));
        Assert.NotNull(mapping);
        Assert.IsType<RawTypedIdConverter<Guid, GuidId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(RawTypedIdComparer<Guid, GuidId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(GuidId?), mapping.ClrType);
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void OptionalRawString_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<StringId?>().AreStringTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(StringId?));
        Assert.NotNull(mapping);
        Assert.IsType<RawStringTypedIdConverter<StringId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(RawStringTypedIdComparer<StringId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(StringId?), mapping.ClrType);
        Assert.Equal(StringId.MaxLength, mapping.GetMaxLength());
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void OptionalPrefixedGuid_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<PrefixedGuidId?>().ArePrefixedTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(PrefixedGuidId?));
        Assert.NotNull(mapping);
        Assert.IsType<PrefixedTypedIdConverter<PrefixedGuidId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(PrefixedTypedIdComparer<PrefixedGuidId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(PrefixedGuidId?), mapping.ClrType);
        Assert.Equal(PrefixedGuidId.MaxLength, mapping.GetMaxLength());
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    [Fact]
    public void OptionalPrefixedString_convention_is_registered_properly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.Properties<PrefixedStringId?>().ArePrefixedTypedId();
        var model = builder.Build();

        var mapping = model.FindProperty(typeof(PrefixedStringId?));
        Assert.NotNull(mapping);
        Assert.IsType<PrefixedTypedIdConverter<PrefixedStringId>>(mapping.GetValueConverter());
        Assert.Equal(typeof(PrefixedTypedIdComparer<PrefixedStringId>), mapping["ValueComparerType"]);
        Assert.Equal(typeof(PrefixedStringId?), mapping.ClrType);
        Assert.Equal(PrefixedStringId.MaxLength, mapping.GetMaxLength());
        Assert.Null(mapping["Relational:ColumnType"]);
    }

    private static void AssertConvertsInt(IntId id)
    {
        var toResult = RawTypedIdConverter<int, IntId>.Instance.ConvertToProvider(id);
        var fromResult = RawTypedIdConverter<int, IntId>.Instance.ConvertFromProvider(id.Value);
        Assert.Equal(id.Value, toResult);
        Assert.Equal(id, fromResult);
    }

    private static void AssertConvertsLong(LongId id)
    {
        var toResult = RawTypedIdConverter<long, LongId>.Instance.ConvertToProvider(id);
        var fromResult = RawTypedIdConverter<long, LongId>.Instance.ConvertFromProvider(id.Value);
        Assert.Equal(id.Value, toResult);
        Assert.Equal(id, fromResult);
    }

    private static void AssertConvertsGuid(GuidId id)
    {
        var toResult = RawTypedIdConverter<Guid, GuidId>.Instance.ConvertToProvider(id);
        var fromResult = RawTypedIdConverter<Guid, GuidId>.Instance.ConvertFromProvider(id.Value);
        Assert.Equal(id.Value, toResult);
        Assert.Equal(id, fromResult);
    }

    private static void AssertConvertsRawString(StringId id)
    {
        var toResult = RawStringTypedIdConverter<StringId>.Instance.ConvertToProvider(id);
        var fromResult = RawStringTypedIdConverter<StringId>.Instance.ConvertFromProvider(id.Value);
        Assert.Equal(id.Value, toResult);
        Assert.Equal(id, fromResult);
    }

    private static void AssertConvertsPrefixedGuid(PrefixedGuidId id)
    {
        var toResult = PrefixedTypedIdConverter<PrefixedGuidId>.Instance.ConvertToProvider(id);
        var fromResult = PrefixedTypedIdConverter<PrefixedGuidId>.Instance.ConvertFromProvider(id.Value);
        Assert.Equal(id.Value, toResult);
        Assert.Equal(id, fromResult);
    }

    private static void AssertConvertsPrefixedString(PrefixedStringId id)
    {
        var toResult = PrefixedTypedIdConverter<PrefixedStringId>.Instance.ConvertToProvider(id);
        var fromResult = PrefixedTypedIdConverter<PrefixedStringId>.Instance.ConvertFromProvider(id.Value);
        Assert.Equal(id.Value, toResult);
        Assert.Equal(id, fromResult);
    }
}
