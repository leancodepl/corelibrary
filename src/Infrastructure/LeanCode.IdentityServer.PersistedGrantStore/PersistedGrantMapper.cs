using IdentityServer4.Models;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    public static class PersistedGrantMapper
    {
        public static void Map(PersistedGrantEntity from, PersistedGrant to)
        {
            if (from is null)
            {
                return;
            }

            to.ClientId = from.ClientId;
            to.CreationTime = from.CreationTime;
            to.Data = from.Data;
            to.Expiration = from.Expiration;
            to.Key = from.Key;
            to.SubjectId = from.SubjectId;
            to.Type = from.Type;
        }

        public static PersistedGrant MapToModel(PersistedGrantEntity entity)
        {
            if (entity is null)
            {
                return null;
            }

            return new PersistedGrant
            {
                ClientId = entity.ClientId,
                CreationTime = entity.CreationTime,
                Data = entity.Data,
                Expiration = entity.Expiration,
                Key = entity.Key,
                SubjectId = entity.SubjectId,
                Type = entity.Type,
            };
        }

        public static PersistedGrantEntity MapToEntity(PersistedGrant grant)
        {
            if (grant is null)
            {
                return null;
            }

            return new PersistedGrantEntity
            {
                ClientId = grant.ClientId,
                CreationTime = grant.CreationTime,
                Data = grant.Data,
                Expiration = grant.Expiration,
                Key = grant.Key,
                SubjectId = grant.SubjectId,
                Type = grant.Type,
            };
        }
    }
}
