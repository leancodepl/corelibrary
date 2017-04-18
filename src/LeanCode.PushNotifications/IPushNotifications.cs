using System;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotifications<TUserId>
    {
        Task Send(TUserId to, DeviceType device, PushNotification notification);
        Task SendToAll(TUserId to, PushNotification notification);
    }

    public interface IPushNotifications : IPushNotifications<Guid>
    { }
}
