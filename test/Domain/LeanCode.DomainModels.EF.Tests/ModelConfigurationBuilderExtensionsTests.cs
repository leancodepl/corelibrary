using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

[SuppressMessage("?", "EF1001", Justification = "Tests.")]
public class ModelConfigurationBuilderExtensionsTests
{
    [Fact]
    public void ConfigureTypedIdsConventions_registers_all_typed_ids_from_assembly()
    {
        var builder = new ModelConfigurationBuilderWrapper();
        builder.ConfigureTypedIdsConventions(typeof(IntId).Assembly);
        var model = builder.Build();

        AssertRegistered<IntId, RawTypedIdConverter<int, IntId>>(model);
        AssertRegistered<LongId, RawTypedIdConverter<long, LongId>>(model);
        AssertRegistered<GuidId, RawTypedIdConverter<Guid, GuidId>>(model);
        AssertRegistered<StringId, RawStringTypedIdConverter<StringId>>(model);
        AssertRegistered<PrefixedGuidId, PrefixedTypedIdConverter<PrefixedGuidId>>(model);
        AssertRegistered<PrefixedUlidId, PrefixedTypedIdConverter<PrefixedUlidId>>(model);
        AssertRegistered<PrefixedStringId, PrefixedTypedIdConverter<PrefixedStringId>>(model);

        AssertRegistered<IntId?, RawTypedIdConverter<int, IntId>>(model);
        AssertRegistered<LongId?, RawTypedIdConverter<long, LongId>>(model);
        AssertRegistered<GuidId?, RawTypedIdConverter<Guid, GuidId>>(model);
        AssertRegistered<StringId?, RawStringTypedIdConverter<StringId>>(model);
        AssertRegistered<PrefixedGuidId?, PrefixedTypedIdConverter<PrefixedGuidId>>(model);
        AssertRegistered<PrefixedUlidId?, PrefixedTypedIdConverter<PrefixedUlidId>>(model);
        AssertRegistered<PrefixedStringId?, PrefixedTypedIdConverter<PrefixedStringId>>(model);
    }

    private static void AssertRegistered<T, TConverter>(ModelConfiguration model)
    {
        var mapping = model.FindProperty(typeof(T));
        Assert.NotNull(mapping);
        Assert.IsType<TConverter>(mapping.GetValueConverter());
    }
}
