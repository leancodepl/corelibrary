using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotificationTokenStore<TUserId>
    {
        Task<List<PushNotificationToken<TUserId>>> GetForDevice(TUserId userId, DeviceType deviceType);
        Task<List<PushNotificationToken<TUserId>>> GetAll(TUserId userId);

        Task UpdateOrAddToken(TUserId userId, DeviceType type, string newToken);
        Task UpdateToken(PushNotificationToken<TUserId> existing, string newToken);
        Task RemoveToken(PushNotificationToken<TUserId> token);
    }
}
