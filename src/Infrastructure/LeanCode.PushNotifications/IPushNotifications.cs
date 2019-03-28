using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotifications<TUserId>
    {
        Task Send(TUserId to, DeviceType device, PushNotification notification);
        Task SendToAll(TUserId to, PushNotification notification);
        PushNotificationBuilder<TUserId> New(TUserId to);
        PushNotificationBuilder<TUserId> New(TUserId to, string cultureName);
    }
}
