using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotificationTokenStore<TUserId>
    {
        Task<List<PushNotificationToken<TUserId>>> GetForDevice(TUserId userId, DeviceType deviceType);
        Task<List<PushNotificationToken<TUserId>>> GetAll(TUserId userId);

        Task UpdateOrAddToken(PushNotificationToken<TUserId> token, string newToken);
        Task RemoveInvalidToken(PushNotificationToken<TUserId> token);
    }
}
