using System;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    public interface IPushNotifications<TUserId>
    {
        Task SendToAllDevices(TUserId to, PushNotification notification);
        Task Send(TUserId to, DeviceType device, PushNotification notification);

        /// <remarks>
        /// <see cref="FCMNotification.To" /> property will be overriden with correct value.
        /// </remarks>
        Task Send(TUserId to, FCMAndroidNotification notification);

        /// <remarks>
        /// <see cref="FCMNotification.To" /> property will be overriden with correct value.
        /// </remarks>
        Task Send(TUserId to, FCMiOSNotification notification);
    }

    public interface IPushNotifications : IPushNotifications<Guid>
    { }
}
