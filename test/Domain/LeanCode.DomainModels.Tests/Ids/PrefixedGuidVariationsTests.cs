using LeanCode.DomainModels.Ids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "cp")]
public readonly partial record struct CustomPrefixId;

[TypedId(TypedIdFormat.PrefixedGuid)]
public readonly partial record struct NormalPrefixWithId;

[TypedId(TypedIdFormat.PrefixedGuid)]
public readonly partial record struct NormalPrefixWithoutIdAtTheEnd;

[TypedId(TypedIdFormat.PrefixedGuid, CustomGenerator = "Guid.Parse(\"f71f9628-3b07-43fd-abcd-4c641d280f39\")")]
public readonly partial record struct CustomGenerator;

public class PrefixedGuidVariationsTests
{
    [Fact]
    public void CustomPrefix_starts_with_custom_prefix()
    {
        Assert.StartsWith("cp_", CustomPrefixId.New().Value, StringComparison.Ordinal);
    }

    [Fact]
    public void NormalPrefixWithId_starts_with_class_name_without_id()
    {
        Assert.StartsWith("normalprefixwith_", NormalPrefixWithId.New().Value, StringComparison.Ordinal);
    }

    [Fact]
    public void NormalPrefixWithoutIdAtTheEnd_starts_with_class_name_as_is()
    {
        Assert.StartsWith(
            "normalprefixwithoutidattheend_",
            NormalPrefixWithoutIdAtTheEnd.New().Value,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Custom_generator_uses_the_expression_specified_as_New_generator()
    {
        Assert.Equal("customgenerator_f71f96283b0743fdabcd4c641d280f39", CustomGenerator.New().Value);
        Assert.Equal("customgenerator_f71f96283b0743fdabcd4c641d280f39", CustomGenerator.New().Value);
        Assert.Equal("customgenerator_f71f96283b0743fdabcd4c641d280f39", CustomGenerator.New().Value);
        Assert.Equal("customgenerator_f71f96283b0743fdabcd4c641d280f39", CustomGenerator.New().Value);
    }
}
