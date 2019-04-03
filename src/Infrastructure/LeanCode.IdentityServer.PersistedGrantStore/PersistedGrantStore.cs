using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    // Source: https://github.com/IdentityServer/IdentityServer4.EntityFramework/blob/dev/src/IdentityServer4.EntityFramework/Stores/PersistedGrantStore.cs
    class PersistedGrantStore : IPersistedGrantStore
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PersistedGrantStore>();
        private readonly IPersistedGrantContext dbContext;
        private readonly IMapper mapper;

        public PersistedGrantStore(IPersistedGrantContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public async Task StoreAsync(PersistedGrant token)
        {
            var existing = await dbContext.PersistedGrants
                .SingleOrDefaultAsync(x => x.Key == token.Key)
                .ConfigureAwait(false);

            if (existing == null)
            {
                logger.Debug("{PersistedGrantKey} not found in database", token.Key);

                var persistedGrant = mapper.Map<PersistedGrantEntity>(token);
                dbContext.PersistedGrants.Add(persistedGrant);
            }
            else
            {
                logger.Debug("{PersistedGrantKey} found in database", token.Key);

                mapper.Map(existing, token);
            }

            try
            {
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Warning(ex, "Exception updating {PersistedGrantKey} persisted grant in database", token.Key);
            }
        }

        public async Task<PersistedGrant> GetAsync(string key)
        {
            var persistedGrant = await dbContext.PersistedGrants
                .FirstOrDefaultAsync(x => x.Key == key)
                .ConfigureAwait(false);

            var model = mapper.Map<PersistedGrant>(persistedGrant);

            logger.Debug("{PersistedGrantKey} found in database: {PersistedGrantKeyFound}", key, model != null);

            return model;
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            var persistedGrants = await dbContext.PersistedGrants
                .Where(x => x.SubjectId == subjectId)
                .ToListAsync()
                .ConfigureAwait(false);
            var model = persistedGrants.Select(x => mapper.Map<PersistedGrant>(x)).ToList();

            logger.Debug("{PersistedGrantCount} persisted grants found for {SubjectId}", persistedGrants.Count, subjectId);

            return model;
        }

        public async Task RemoveAsync(string key)
        {
            var persistedGrant = await dbContext.PersistedGrants
                .FirstOrDefaultAsync(x => x.Key == key)
                .ConfigureAwait(false);
            if (persistedGrant != null)
            {
                logger.Debug("Removing {PersistedGrantKey} persisted grant from database", key);

                dbContext.PersistedGrants.Remove(persistedGrant);

                try
                {
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.Warning(ex, "Exception removing {PersistedGrantKey} persisted grant from database", key);
                }
            }
            else
            {
                logger.Debug("No {PersistedGrantKey} persisted grant found in database", key);
            }
        }

        public async Task RemoveAllAsync(string subjectId, string clientId)
        {
            var persistedGrants = await dbContext.PersistedGrants
                .Where(x => x.SubjectId == subjectId && x.ClientId == clientId)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.Debug("Removing {PersistedGrantCount} persisted grants from database for subject {SubjectId}, clientId {ClientId}", persistedGrants.Count, subjectId, clientId);

            dbContext.PersistedGrants.RemoveRange(persistedGrants);

            try
            {
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Warning(ex, "Exception removing {PersistedGrantCount} persisted grants from database for subject {SubjectId}, clientId {ClientId}", persistedGrants.Count, subjectId, clientId);
            }
        }

        public async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            var persistedGrants = await dbContext.PersistedGrants
                .Where(x =>
                    x.SubjectId == subjectId &&
                    x.ClientId == clientId &&
                    x.Type == type)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.Debug("Removing {PersistedGrantCount} persisted grants from database for subject {SubjectId}, clientId {ClientId}, grantType {PersistedGrantType}", persistedGrants.Count, subjectId, clientId, type);

            dbContext.PersistedGrants.RemoveRange(persistedGrants);

            try
            {
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Warning(ex, "Exception removing {PersistedGrantCount} persisted grants from database for subject {SubjectId}, clientId {ClientId}, grantType {PersistedGrantType}", persistedGrants.Count, subjectId, clientId, type);
            }
        }
    }
}
