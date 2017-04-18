using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotificationTokenStore<TUserId>
    {
        Task<PushNotificationToken<TUserId>> GetToken(TUserId userId, DeviceType deviceType);
        Task<List<PushNotificationToken<TUserId>>> GetAll(TUserId userId);

        Task UpdateOrAddToken(TUserId userId, DeviceType deviceType, string newToken);
        Task RemoveInvalidToken(TUserId userId, DeviceType deviceType);
    }

    public interface IPushNotificationTokenStore : IPushNotificationTokenStore<Guid>
    { }
}
