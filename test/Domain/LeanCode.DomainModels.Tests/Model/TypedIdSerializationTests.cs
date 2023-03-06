using System.Text.Json;
using LeanCode.DomainModels.Model;
using Xunit;

namespace LeanCode.DomainModels.Tests.Model;

public class TypedIdSerializationTests
{
    private record Entity(Id<Entity> Id) : IIdentifiable<Id<Entity>>;

    private record IntEntity(IId<IntEntity> Id) : IIdentifiable<IId<IntEntity>>;

    private record LongEntity(LId<LongEntity> Id) : IIdentifiable<LId<LongEntity>>;

    private record StrEntity(SId<StrEntity> Id) : IIdentifiable<SId<StrEntity>>;

    [IdSlug("custom")]
    private class StringOverriddenEntity : IIdentifiable<SId<StringOverriddenEntity>>
    {
        public SId<StringOverriddenEntity> Id { get; set; }
    }

    [Fact]
    public void Serializes_and_deserializes_non_nullable_id()
    {
        var idString = "00000000-0000-0000-0000-000000000001";
        var id = Id<Entity>.From(Guid.Parse(idString));

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<Id<Entity>>(json);

        Assert.Equal('"' + idString + '"', json);
        Assert.Equal(id, deserialized);
    }

    [Fact]
    public void Serializes_and_deserializes_nullable_id()
    {
        var idString = "00000000-0000-0000-0000-000000000001";
        Id<Entity>? id = Id<Entity>.From(Guid.Parse(idString));

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<Id<Entity>?>(json);

        Assert.Equal('"' + idString + '"', json);
        Assert.Equal(id, deserialized);
    }

    [Fact]
    public void Serializes_and_deserializes_null_id()
    {
        Id<Entity>? id = null;

        var json = JsonSerializer.Serialize(id);

        var deserialized = JsonSerializer.Deserialize<Id<Entity>?>(json);

        Assert.Equal("null", json);
        Assert.Equal(id, deserialized);
    }

    [Fact]
    public void Serializes_and_deserializes_id_as_dictionary_keys()
    {
        var idString = "00000000-0000-0000-0000-000000000001";
        var id = Id<Entity>.From(Guid.Parse(idString));

        var dict = new Dictionary<Id<Entity>, int> {[id] = 1};
        var expectedJson = "{\"00000000-0000-0000-0000-000000000001\":1}";

        var json = JsonSerializer.Serialize(dict);
        var deserialized = JsonSerializer.Deserialize<Dictionary<Id<Entity>, int>>(json);

        Assert.Equal(expectedJson, json);
        Assert.Equal(dict, deserialized);
    }

    [Fact]
    public void Throws_when_id_is_not_a_valid_guid()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Id<Entity>?>("notaguid"));
    }

    [Fact]
    public void Serializes_and_deserializes_int_id()
    {
        var id = IId<IntEntity>.From(7);

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<IId<IntEntity>>(json);

        Assert.Equal("7", json);
        Assert.Equal(id, deserialized);
    }

    [Fact]
    public void Serializes_and_deserializes_int_id_as_dictionary_keys()
    {
        var id = IId<IntEntity>.From(7);

        var dict = new Dictionary<IId<IntEntity>, int> {[id] = 1};
        var expectedJson = "{\"7\":1}";

        var json = JsonSerializer.Serialize(dict);
        var deserialized = JsonSerializer.Deserialize<Dictionary<IId<IntEntity>, int>>(json);

        Assert.Equal(expectedJson, json);
        Assert.Equal(dict, deserialized);
    }

    [Fact]
    public void Throws_when_id_is_not_a_valid_int()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IId<IntEntity>?>("213452343243242343452"));
    }

    [Fact]
    public void Serializes_and_deserializes_long_id()
    {
        var id = LId<LongEntity>.From(922337203685477580);

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<LId<LongEntity>>(json);

        Assert.Equal("922337203685477580", json);
        Assert.Equal(id, deserialized);
    }

    [Fact]
    public void Serializes_and_deserializes_long_id_as_dictionary_keys()
    {
        var id = LId<LongEntity>.From(922337203685477580);

        var dict = new Dictionary<LId<LongEntity>, int> {[id] = 1};
        var expectedJson = "{\"922337203685477580\":1}";

        var json = JsonSerializer.Serialize(dict);
        var deserialized = JsonSerializer.Deserialize<Dictionary<LId<LongEntity>, int>>(json);

        Assert.Equal(expectedJson, json);
        Assert.Equal(dict, deserialized);
    }

    [Fact]
    public void Throws_when_id_is_not_a_valid_long()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<LId<LongEntity>?>("21345.3452"));
    }

    [Fact]
    public void Serializes_and_deserializes_sid()
    {
        var guid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var id = new SId<StrEntity>(guid);

        var expectedJson = "\"strentity_00000000000000000000000000000001\"";

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<SId<StrEntity>>(json);

        Assert.Equal(expectedJson, json);
        Assert.Equal(id, deserialized);
    }

    [Fact]
    public void Serializes_and_deserializes_sid_as_dictionary_keys()
    {
        var guid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var id = new SId<StrEntity>(guid);

        var dict = new Dictionary<SId<StrEntity>, int> {[id] = 1};
        var expectedJson = "{\"strentity_00000000000000000000000000000001\":1}";

        var json = JsonSerializer.Serialize(dict);
        var deserialized = JsonSerializer.Deserialize<Dictionary<SId<StrEntity>, int>>(json);

        Assert.Equal(expectedJson, json);
        Assert.Equal(dict, deserialized);
    }

    [Fact]
    public void Allows_overriding_sid_prefix()
    {
        var guid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var id = new SId<StringOverriddenEntity>(guid);

        var expectedJson = "\"custom_00000000000000000000000000000001\"";

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<SId<StringOverriddenEntity>>(json);

        Assert.Equal(expectedJson, json);
        Assert.Equal(id, deserialized);
    }
}
