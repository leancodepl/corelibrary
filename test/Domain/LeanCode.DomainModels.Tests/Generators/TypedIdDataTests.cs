using LeanCode.DomainModels.Generators;
using Microsoft.CodeAnalysis;
using Xunit;
using TypedIdFormat = LeanCode.DomainModels.Generators.TypedIdFormat;

namespace LeanCode.DomainModels.Tests.Generators;

public class TypedIdDataTests
{
    [Fact]
    public void Equality_returns_true_when_all_properties_match()
    {
        var data1 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace",
            "TestId",
            null,
            false,
            null,
            true,
            null
        );

        var data2 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace",
            "TestId",
            null,
            false,
            null,
            true,
            null
        );

        Assert.Equal(data1, data2);
        Assert.True(data1 == data2);
        Assert.False(data1 != data2);
    }

    [Fact]
    public void Equality_returns_false_when_format_differs()
    {
        var data1 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace",
            "TestId",
            null,
            false,
            null,
            true,
            null
        );

        var data2 = new TypedIdData(TypedIdFormat.RawGuid, "Test.Namespace", "TestId", null, false, null, true, null);

        Assert.NotEqual(data1, data2);
        Assert.False(data1 == data2);
        Assert.True(data1 != data2);
    }

    [Fact]
    public void Equality_returns_false_when_namespace_differs()
    {
        var data1 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace1",
            "TestId",
            null,
            false,
            null,
            true,
            null
        );

        var data2 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace2",
            "TestId",
            null,
            false,
            null,
            true,
            null
        );

        Assert.NotEqual(data1, data2);
    }

    [Fact]
    public void Equality_returns_false_when_type_name_differs()
    {
        var data1 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace",
            "TestId1",
            null,
            false,
            null,
            true,
            null
        );

        var data2 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace",
            "TestId2",
            null,
            false,
            null,
            true,
            null
        );

        Assert.NotEqual(data1, data2);
    }

    [Fact]
    public void Equality_ignores_location_property()
    {
        var location1 = Location.None;
        var location2 = Location.Create("test.cs", default, default);

        var data1 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace",
            "TestId",
            null,
            false,
            null,
            true,
            location1
        );

        var data2 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace",
            "TestId",
            null,
            false,
            null,
            true,
            location2
        );

        Assert.Equal(data1, data2);
    }

    [Fact]
    public void GetHashCode_returns_same_value_for_equal_instances()
    {
        var data1 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace",
            "TestId",
            "custom_",
            true,
            100,
            true,
            null
        );

        var data2 = new TypedIdData(
            TypedIdFormat.PrefixedGuid,
            "Test.Namespace",
            "TestId",
            "custom_",
            true,
            100,
            true,
            null
        );

        Assert.Equal(data1.GetHashCode(), data2.GetHashCode());
    }
}
