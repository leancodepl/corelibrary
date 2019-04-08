using System;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotifications<TUserId>
    {
        PushNotificationBuilder<TUserId> New();
        LocalizedPushNotificationBuilder<TUserId> Localized(string cultureName);

        Task SendAsync(TUserId to, DeviceType device, PushNotification notification);
        Task SendToAllAsync(TUserId to, PushNotification notification);
    }
}
