using FluentAssertions;
using LeanCode.DomainModels.Ids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.PrefixedString, CustomPrefix = "cp", MaxValueLength = 50)]
public readonly partial record struct CustomPrefixStringId;

[TypedId(TypedIdFormat.PrefixedString, MaxValueLength = 50)]
public readonly partial record struct NormalStringPrefixWithId;

[TypedId(TypedIdFormat.PrefixedString, MaxValueLength = 50)]
public readonly partial record struct NormalStringPrefixWithoutIdAtTheEnd;

public class PrefixedStringVariationsTests
{
    [Fact]
    public void CustomPrefix_starts_with_custom_prefix()
    {
        CustomPrefixStringId.FromValuePart("test").Value.Should().StartWith("cp_");
    }

    [Fact]
    public void NormalPrefixWithId_starts_with_class_name_without_id()
    {
        Assert.StartsWith(
            "normalstringprefixwith_",
            NormalStringPrefixWithId.FromValuePart("test").Value,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void NormalPrefixWithoutIdAtTheEnd_starts_with_class_name_as_is()
    {
        Assert.StartsWith(
            "normalstringprefixwithoutidattheend_",
            NormalStringPrefixWithoutIdAtTheEnd.FromValuePart("test").Value,
            StringComparison.Ordinal
        );
    }
}
