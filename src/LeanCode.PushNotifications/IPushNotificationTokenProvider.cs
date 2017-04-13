using System;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotificationTokenProvider<TUserId>
    {
        Task<PushNotificationToken<TUserId>> GetToken(TUserId userId, DeviceType deviceType);
        Task UpdateOrAddToken(TUserId userId, DeviceType deviceType);
    }

    public interface IPushNotificationTokenProvider : IPushNotificationTokenProvider<Guid>
    { }
}
