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
                .ToListAsync()
                .ConfigureAwait(false);

            return entities
                .Select(e => new PushNotificationToken<Guid>(e.Id, e.UserId, e.DeviceType, e.Token))
                .ToList();
        }

        public async Task<List<PushNotificationToken<Guid>>> GetAll(Guid userId)
        {
            var entities = await dbSet
                .Where(e => e.UserId == userId)
                .ToListAsync()
                .ConfigureAwait(false);

            return entities
                .Select(e => new PushNotificationToken<Guid>(e.Id, e.UserId, e.DeviceType, e.Token))
                .ToList();
        }

        public async Task RemoveInvalidToken(PushNotificationToken<Guid> token)
        {
            RemoveToken(token.Id);
            await unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateOrAddToken(PushNotificationToken<Guid> token, string newToken)
        {
            RemoveToken(token.Id);
            dbSet.Add(new PushNotificationTokenEntity
            {
                Id = Guid.NewGuid(),
                UserId = token.UserId,
                DeviceType = token.DeviceType,
                Token = newToken,
                DateCreated = Time.Now
            });
            await unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private void RemoveToken(Guid tokenId)
        {
            var entity = new PushNotificationTokenEntity { Id = tokenId };
            dbSet.Attach(entity);
            dbSet.Remove(entity);
        }
    }
}
