using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.PushNotifications.EF
{
    public sealed class EFPushNotificationTokenStore : IPushNotificationTokenStore<Guid>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<EFPushNotificationTokenStore>();
        private readonly DbContext unitOfWork;
        private readonly DbSet<PushNotificationTokenEntity> dbSet;

        public EFPushNotificationTokenStore(DbContext unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            this.dbSet = unitOfWork.Set<PushNotificationTokenEntity>();
        }

        public async Task<List<PushNotificationToken<Guid>>> GetForDevice(Guid userId, DeviceType deviceType)
        {
            var entities = await dbSet
                .Where(e => e.UserId == userId && e.DeviceType == deviceType)
                .ToListAsync();

            return entities
                .Select(e => new PushNotificationToken<Guid>(e.Id, e.UserId, e.DeviceType, e.Token))
                .ToList();
        }

        public async Task<List<PushNotificationToken<Guid>>> GetAll(Guid userId)
        {
            var entities = await dbSet
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return entities
                .Select(e => new PushNotificationToken<Guid>(e.Id, e.UserId, e.DeviceType, e.Token))
                .ToList();
        }

        public async Task RemoveToken(PushNotificationToken<Guid> token)
        {
            await RemoveTokenById(token.Id);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveForDevice(Guid userId, DeviceType deviceType)
        {
            var entities = await dbSet
                .Where(e => e.UserId == userId && e.DeviceType == deviceType).ToListAsync();

            dbSet.RemoveRange(entities);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateOrAddToken(Guid userId, DeviceType deviceType, string newToken)
        {
            var existing = await LoadExisting(userId, deviceType, newToken);
            if (existing is null)
            {
                dbSet.Add(new PushNotificationTokenEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DeviceType = deviceType,
                    Token = newToken,
                    DateCreated = Time.Now
                });
            }

            try
            {
                await unitOfWork.SaveChangesAsync();
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
                        case 2601:  // Duplicated key row error
                            logger.Verbose("Duplicate token received for user {UserId} on device {DeviceType}", userId, deviceType.ToString());
                            break;
                    }
                }
            }
        }

        public async Task UpdateToken(PushNotificationToken<Guid> token, string newToken)
        {
            await RemoveTokenById(token.Id);
            await UpdateOrAddToken(token.UserId, token.DeviceType, newToken);
        }

        private async Task RemoveTokenById(Guid tokenId)
        {
            var entity = await dbSet.FindAsync(tokenId);
            dbSet.Remove(entity);
        }

        private Task<PushNotificationTokenEntity> LoadExisting(Guid userId, DeviceType deviceType, string newToken)
        {
            return dbSet.FirstOrDefaultAsync(e => e.UserId == userId && e.DeviceType == deviceType && e.Token == newToken);
        }
    }
}
