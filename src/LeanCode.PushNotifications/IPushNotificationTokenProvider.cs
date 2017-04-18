using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotificationTokenProvider<TUserId>
    {
        Task<PushNotificationToken<TUserId>> GetToken(TUserId userId, DeviceType deviceType);
        Task<List<PushNotificationToken<TUserId>>> GetAll(TUserId userId);

        Task UpdateOrAddToken(TUserId userId, DeviceType deviceType);
    }

    public interface IPushNotificationTokenProvider : IPushNotificationTokenProvider<Guid>
    { }
}
