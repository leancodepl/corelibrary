using LeanCode.DomainModels.Ids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "cp")]
public readonly partial record struct CustomPrefixId;

[TypedId(TypedIdFormat.PrefixedGuid)]
public readonly partial record struct NormalPrefixWithId;

[TypedId(TypedIdFormat.PrefixedGuid)]
public readonly partial record struct NormalPrefixWithoutIdAtTheEnd;

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
}
