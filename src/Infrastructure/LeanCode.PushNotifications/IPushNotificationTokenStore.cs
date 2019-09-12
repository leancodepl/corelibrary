using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotificationTokenStore<TUserId>
    {
        Task<List<PushNotificationToken<TUserId>>> GetForDeviceAsync(TUserId userId, DeviceType deviceType);
        Task<List<PushNotificationToken<TUserId>>> GetAllAsync(TUserId userId);

        Task UpdateOrAddTokenAsync(TUserId userId, DeviceType type, string newToken);
        Task UpdateTokenAsync(PushNotificationToken<TUserId> existing, string newToken);
        Task RemoveForDeviceAsync(TUserId userId, DeviceType deviceType);
        Task RemoveTokenAsync(PushNotificationToken<TUserId> token);
    }
}
