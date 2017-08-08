using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.TimeProvider;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.PushNotifications.EF
{
    public sealed class EFPushNotificationTokenStore : IPushNotificationTokenStore<Guid>
    {
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

        public async Task RemoveInvalidToken(PushNotificationToken<Guid> token)
        {
            await RemoveToken(token.Id);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateOrAddToken(Guid userId, DeviceType deviceType, string newToken)
        {
            var existing = await LoadExisting(userId, deviceType, newToken);
            if (existing == null)
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
            await unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateToken(PushNotificationToken<Guid> token, string newToken)
        {
            await RemoveToken(token.Id);
            await UpdateOrAddToken(token.UserId, token.DeviceType, newToken);
        }

        private async Task RemoveToken(Guid tokenId)
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
