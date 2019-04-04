using System;
using IdentityServer4.Models;
using NSubstitute;
using Xunit;

namespace LeanCode.IdentityServer.PersistedGrantStore.Tests
{
    public class PersistedGrantMapperTests
    {
        private static readonly string clientId = "id";
        private static readonly DateTime creationTime = new DateTime(2019, 4, 4);
        private static readonly string data = "data";
        private static readonly DateTime? expiration = new DateTime(2019, 4, 5);
        private static readonly string key = "key";
        private static readonly string subjectId = "subjectId";
        private static readonly string type = "type";
        private static readonly string clientIdNew = "idNew";
        private static readonly DateTime creationTimeNew = new DateTime(2019, 3, 4);
        private static readonly string dataNew = "dataNew";
        private static readonly DateTime? expirationNew = null;
        private static readonly string keyNew = "keyNew";
        private static readonly string subjectIdNew = "subjectIdNew";
        private static readonly string typeNew = "typeNew";

        [Fact]
        public void Succeeds_mapping_entity_to_model()
        {
            PersistedGrantEntity entity = new PersistedGrantEntity()
            {
                ClientId = clientId,
                CreationTime = creationTime,
                Data = data,
                Expiration = expiration,
                Key = key,
                SubjectId = subjectId,
                Type = type
            };
            var model = PersistedGrantMapper.MapToModel(entity);

            Assert.Equal(entity.ClientId, clientId);
            Assert.Equal(entity.CreationTime, creationTime);
            Assert.Equal(entity.Data, data);
            Assert.Equal(entity.Expiration, expiration);
            Assert.Equal(entity.Key, key);
            Assert.Equal(entity.SubjectId, subjectId);
            Assert.Equal(entity.Type, type);
        }

        [Fact]
        public void Succeeds_mapping_model_to_entity()
        {
            PersistedGrant model = new PersistedGrant()
            {
                ClientId = clientId,
                CreationTime = creationTime,
                Data = data,
                Expiration = expiration,
                Key = key,
                SubjectId = subjectId,
                Type = type
            };
            var entity = PersistedGrantMapper.MapToEntity(model);

            Assert.Equal(model.ClientId, clientId);
            Assert.Equal(model.CreationTime, creationTime);
            Assert.Equal(model.Data, data);
            Assert.Equal(model.Expiration, expiration);
            Assert.Equal(model.Key, key);
            Assert.Equal(model.SubjectId, subjectId);
            Assert.Equal(model.Type, type);
        }

        [Fact]
        public void Succeeds_mapping_entity_to_existing_model_object()
        {
            PersistedGrantEntity entity = new PersistedGrantEntity()
            {
                ClientId = clientIdNew,
                CreationTime = creationTimeNew,
                Data = dataNew,
                Expiration = expirationNew,
                Key = keyNew,
                SubjectId = subjectIdNew,
                Type = typeNew
            };

            PersistedGrant model = new PersistedGrant()
            {
                ClientId = clientId,
                CreationTime = creationTime,
                Data = data,
                Expiration = expiration,
                Key = key,
                SubjectId = subjectId,
                Type = type
            };

            PersistedGrantMapper.Map(entity, model);

            Assert.Equal(model.ClientId, clientIdNew);
            Assert.Equal(model.CreationTime, creationTimeNew);
            Assert.Equal(model.Data, dataNew);
            Assert.Equal(model.Expiration, expirationNew);
            Assert.Equal(model.Key, keyNew);
            Assert.Equal(model.SubjectId, subjectIdNew);
            Assert.Equal(model.Type, typeNew);
        }
    }
}
