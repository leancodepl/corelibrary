using System.Globalization;
using FluentAssertions;
using LeanCode.DomainModels.Ids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.PrefixedUlid, CustomPrefix = "cp")]
public readonly partial record struct CustomUlidPrefixId;

[TypedId(TypedIdFormat.PrefixedUlid)]
public readonly partial record struct NormalUlidPrefixWithId;

[TypedId(TypedIdFormat.PrefixedUlid)]
public readonly partial record struct NormalUlidPrefixWithoutIdAtTheEnd;

public class PrefixedUlidVariationsTests
{
    [Fact]
    public void CustomPrefix_starts_with_custom_prefix()
    {
        CustomUlidPrefixId.New().Value.Should().StartWith("cp_");
    }

    [Fact]
    public void NormalPrefixWithId_starts_with_class_name_without_id()
    {
        Assert.StartsWith("normalulidprefixwith_", NormalUlidPrefixWithId.New().Value, StringComparison.Ordinal);
    }

    [Fact]
    public void NormalPrefixWithoutIdAtTheEnd_starts_with_class_name_as_is()
    {
        Assert.StartsWith(
            "normalulidprefixwithoutidattheend_",
            NormalUlidPrefixWithoutIdAtTheEnd.New().Value,
            StringComparison.Ordinal
        );
    }
}
