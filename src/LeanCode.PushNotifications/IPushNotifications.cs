using System;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotifications<TUserId>
    {
        Task SendToAllDevices(TUserId to, PushNotification notification);
        Task Send(TUserId to, DeviceType device, PushNotification notification);
    }

    public interface IPushNotifications : IPushNotifications<Guid>
    { }
}
