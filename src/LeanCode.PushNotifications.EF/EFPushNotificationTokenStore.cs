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

        public async Task<List<PushNotificationToken<Guid>>> GetAll(Guid userId)
        {
            var entities = await dbSet
                .Where(e => e.UserId == userId)
                .ToListAsync()
                .ConfigureAwait(false);

            return entities
                .Select(e => new PushNotificationToken<Guid>(e.UserId, e.DeviceType, e.Token))
                .ToList();
        }

        public async Task<PushNotificationToken<Guid>> GetToken(Guid userId, DeviceType deviceType)
        {
            var entity = await dbSet.FindAsync(userId, deviceType).ConfigureAwait(false);
            return entity != null ? new PushNotificationToken<Guid>(entity.UserId, entity.DeviceType, entity.Token) : null;
        }

        public async Task RemoveInvalidToken(Guid userId, DeviceType deviceType)
        {
            RemoveToken(userId, deviceType);
            await unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateOrAddToken(Guid userId, DeviceType deviceType, string newToken)
        {
            RemoveToken(userId, deviceType);
            dbSet.Add(new PushNotificationTokenEntity
            {
                UserId = userId,
                DeviceType = deviceType,
                Token = newToken,
                DateCreated = Time.Now
            });
            await unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private void RemoveToken(Guid userId, DeviceType deviceType)
        {
            var entity = new PushNotificationTokenEntity { UserId = userId, DeviceType = deviceType };
            dbSet.Attach(entity);
            dbSet.Remove(entity);
        }
    }
}
