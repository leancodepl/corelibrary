using System;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotifications<TUserId>
    {
        [Obsolete("Use `SendAsync(TUserId, DeviceType, PushNotification)` instead.")]
        Task Send(TUserId to, DeviceType device, PushNotification notification);

        [Obsolete("Use `SendToAllAsync(TUserId, PushNotification)` instead.")]
        Task SendToAll(TUserId to, PushNotification notification);

        PushNotificationBuilder<TUserId> New(TUserId to);
        LocalizedPushNotificationBuilder<TUserId> New(TUserId to, string cultureName);
        Task SendAsync(TUserId to, DeviceType device, PushNotification notification);
        Task SendToAllAsync(TUserId to, PushNotification notification);
    }
}
