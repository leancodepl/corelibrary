using LeanCode.DomainModels.Model;
using Xunit;

namespace LeanCode.DomainModels.Tests.Model;

[Obsolete("Tests for obsolete classes")]
public class SIdTests
{
    internal sealed record Entity(SId<Entity> Id) : IIdentifiable<SId<Entity>>;

    internal sealed record VeryLongEntity(SId<VeryLongEntity> Id) : IIdentifiable<SId<VeryLongEntity>>;

    [IdSlug("cus")]
    internal sealed record CustomEntity(SId<CustomEntity> Id) : IIdentifiable<SId<CustomEntity>>;

    [Fact]
    public void Parses_and_stringifies_correctly()
    {
        var guid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var expected = "entity_00000000000000000000000000000001";

        Assert.Equal(expected, SId<Entity>.From(expected));
        Assert.Equal(expected, SId<Entity>.FromNullable(expected));
        Assert.Equal(expected, SId<Entity>.FromGuidOrSId(expected));
        Assert.Equal(expected, SId<Entity>.FromGuidOrSId(guid.ToString()));
        Assert.Equal(expected, new SId<Entity>(guid));

        Assert.True(SId<Entity>.TryParse(expected, out var tryParsed));
        Assert.Equal(expected, tryParsed);

        Assert.True(SId<Entity>.TryParseFromGuidOrSId(guid.ToString(), out var tryParsedGuid));
        Assert.Equal(expected, tryParsedGuid);

        Assert.True(SId<Entity>.TryParseFromGuidOrSId(expected, out var tryParsedStr));
        Assert.Equal(expected, tryParsedStr);
    }

    [Fact]
    public void Caps_entity_type_prefix_to_10_chars()
    {
        var expected = "verylongen_00000000000000000000000000000001";
        var id = SId<VeryLongEntity>.From(expected);
        Assert.Equal(expected, id);
    }

    [Fact]
    public void Allows_overriding_entity_prefix()
    {
        var expected = "cus_00000000000000000000000000000001";
        var id = SId<CustomEntity>.From(expected);
        Assert.Equal(expected, id);
    }

    [Fact]
    public void Checks_validity_of_sid()
    {
        Assert.True(SId<Entity>.IsValid("entity_00000000000000000000000000000001"));
        Assert.False(SId<Entity>.IsValid("notentity_00000000000000000000000000000001"));
    }

    [Fact]
    public void Equality_operators_work()
    {
        var same1 = SId<Entity>.From("entity_00000000000000000000000000000001");
        var same2 = SId<Entity>.From("entity_00000000000000000000000000000001");
        var other = SId<Entity>.From("entity_00000000000000000000000000000002");

        Assert.True(same1 == same2);
        Assert.True(same1 != other);
        Assert.False(same1 != same2);
        Assert.False(same1 == other);
    }

    [Fact]
    public void Throws_if_malformed_input_is_given()
    {
        Assert.Throws<FormatException>(() => SId<Entity>.From("entity_notaguid"));
        Assert.Throws<FormatException>(() => SId<Entity>.From("notentity_00000000000000000000000000000001"));

        Assert.False(SId<Entity>.TryParse("entity_notaguid", out _));
        Assert.False(SId<Entity>.TryParseFromGuidOrSId("entity_notaguid", out _));
    }

    [Fact]
    public void From_nullable_accepts_null_and_valid_sid()
    {
        Assert.Null(SId<Entity>.FromNullable(null));
        Assert.Equal(
            "entity_00000000000000000000000000000001",
            SId<Entity>.FromNullable("entity_00000000000000000000000000000001")
        );
    }
}
