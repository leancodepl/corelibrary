using System;
using IdentityServer4.Models;
using NSubstitute;
using Xunit;

namespace LeanCode.IdentityServer.PersistedGrantStore.Tests
{
    public class PersistedGrantMapperTests
    {
        private static readonly string ClientId = "id";
        private static readonly DateTime CreationTime = new DateTime(2019, 4, 4);
        private static readonly string Data = "data";
        private static readonly DateTime? Expiration = new DateTime(2019, 4, 5);
        private static readonly string Key = "key";
        private static readonly string SubjectId = "subjectId";
        private static readonly string Type = "type";
        private static readonly string ClientIdNew = "idNew";
        private static readonly DateTime CreationTimeNew = new DateTime(2019, 3, 4);
        private static readonly string DataNew = "dataNew";
        private static readonly DateTime? ExpirationNew = null;
        private static readonly string KeyNew = "keyNew";
        private static readonly string SubjectIdNew = "subjectIdNew";
        private static readonly string TypeNew = "typeNew";

        [Fact]
        public void Succeeds_mapping_entity_to_model()
        {
            var entity = new PersistedGrantEntity()
            {
                ClientId = ClientId,
                CreationTime = CreationTime,
                Data = Data,
                Expiration = Expiration,
                Key = Key,
                SubjectId = SubjectId,
                Type = Type,
            };
            var model = PersistedGrantMapper.MapToModel(entity);

            Assert.Equal(model.ClientId, ClientId);
            Assert.Equal(model.CreationTime, CreationTime);
            Assert.Equal(model.Data, Data);
            Assert.Equal(model.Expiration, Expiration);
            Assert.Equal(model.Key, Key);
            Assert.Equal(model.SubjectId, SubjectId);
            Assert.Equal(model.Type, Type);
        }

        [Fact]
        public void Succeeds_mapping_model_to_entity()
        {
            var model = new PersistedGrant()
            {
                ClientId = ClientId,
                CreationTime = CreationTime,
                Data = Data,
                Expiration = Expiration,
                Key = Key,
                SubjectId = SubjectId,
                Type = Type,
            };
            var entity = PersistedGrantMapper.MapToEntity(model);

            Assert.Equal(entity.ClientId, ClientId);
            Assert.Equal(entity.CreationTime, CreationTime);
            Assert.Equal(entity.Data, Data);
            Assert.Equal(entity.Expiration, Expiration);
            Assert.Equal(entity.Key, Key);
            Assert.Equal(entity.SubjectId, SubjectId);
            Assert.Equal(entity.Type, Type);
        }

        [Fact]
        public void Succeeds_mapping_entity_to_existing_model_object()
        {
            var entity = new PersistedGrantEntity()
            {
                ClientId = ClientIdNew,
                CreationTime = CreationTimeNew,
                Data = DataNew,
                Expiration = ExpirationNew,
                Key = KeyNew,
                SubjectId = SubjectIdNew,
                Type = TypeNew,
            };

            var model = new PersistedGrant()
            {
                ClientId = ClientId,
                CreationTime = CreationTime,
                Data = Data,
                Expiration = Expiration,
                Key = Key,
                SubjectId = SubjectId,
                Type = Type,
            };

            PersistedGrantMapper.Map(entity, model);

            Assert.Equal(model.ClientId, ClientIdNew);
            Assert.Equal(model.CreationTime, CreationTimeNew);
            Assert.Equal(model.Data, DataNew);
            Assert.Equal(model.Expiration, ExpirationNew);
            Assert.Equal(model.Key, KeyNew);
            Assert.Equal(model.SubjectId, SubjectIdNew);
            Assert.Equal(model.Type, TypeNew);
        }
    }
}
