using System.Text.Json;
using LeanCode.DomainModels.Model;
using Xunit;

namespace LeanCode.DomainModels.Tests
{
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
        public void Serializes_and_deserializes_int_id()
        {
            var id = IId<IntEntity>.From(7);

            var json = JsonSerializer.Serialize(id);
            var deserialized = JsonSerializer.Deserialize<IId<IntEntity>>(json);

            Assert.Equal("7", json);
            Assert.Equal(id, deserialized);
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
}
