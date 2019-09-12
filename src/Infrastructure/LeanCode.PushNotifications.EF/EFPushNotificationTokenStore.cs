using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.TimeProvider;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.PushNotifications.EF
{
    public sealed class EFPushNotificationTokenStore : IPushNotificationTokenStore<Guid>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<EFPushNotificationTokenStore>();
        private readonly DbContext dbContext;
        private readonly DbSet<PushNotificationTokenEntity> dbSet;

        public EFPushNotificationTokenStore(DbContext dbContext)
        {
            this.dbContext = dbContext;

            dbSet = dbContext.Set<PushNotificationTokenEntity>();
        }

        public Task<List<PushNotificationToken<Guid>>> GetForDeviceAsync(Guid userId, DeviceType deviceType)
        {
            return dbSet
                .Where(e => e.UserId == userId && e.DeviceType == deviceType)
                .Select(e => new PushNotificationToken<Guid>(e.Id, e.UserId, e.DeviceType, e.Token))
                .ToListAsync();
        }

        public Task<List<PushNotificationToken<Guid>>> GetAllAsync(Guid userId)
        {
            return dbSet
                .Where(e => e.UserId == userId)
                .Select(e => new PushNotificationToken<Guid>(e.Id, e.UserId, e.DeviceType, e.Token))
                .ToListAsync();
        }

        public async Task RemoveTokenAsync(PushNotificationToken<Guid> token)
        {
            await RemoveTokenByIdAsync(token.Id);
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveForDeviceAsync(Guid userId, DeviceType deviceType)
        {
            var entities = await dbSet
                .Where(e => e.UserId == userId && e.DeviceType == deviceType)
                .ToListAsync();

            dbSet.RemoveRange(entities);

            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateOrAddTokenAsync(Guid userId, DeviceType deviceType, string newToken)
        {
            var existing = await LoadExisting(userId, deviceType, newToken);

            if (existing is null)
            {
                dbSet.Add(new PushNotificationTokenEntity()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DeviceType = deviceType,
                    Token = newToken,
                    DateCreated = Time.Now,
                });
            }

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                if (exception?.InnerException?.InnerException is SqlException sqlException)
                {
                    switch (sqlException.Number)
                    {
                        // Constraint violation exception
                        // case 2627:  // Unique constraint error (throws when primary key duplicates)
                        // case 547:   // Constraint check violation
                        case 2601: // Duplicated key row error
                            logger.Verbose(
                                "Duplicate token received for user {UserId} on device {DeviceType}",
                                userId, deviceType.ToString());

                            break;
                    }
                }
            }
        }

        public async Task UpdateTokenAsync(PushNotificationToken<Guid> token, string newToken)
        {
            await RemoveTokenByIdAsync(token.Id);
            await UpdateOrAddTokenAsync(token.UserId, token.DeviceType, newToken);
        }

        private async Task RemoveTokenByIdAsync(Guid tokenId)
        {
            var entity = await dbSet.FindAsync(tokenId);

            dbSet.Remove(entity);
        }

        private Task<PushNotificationTokenEntity> LoadExisting(Guid userId, DeviceType deviceType, string newToken) =>
            dbSet.FirstOrDefaultAsync(e => e.UserId == userId && e.DeviceType == deviceType && e.Token == newToken);
    }
}
