using System;
using System.Threading.Tasks;

namespace LeanCode.PushNotifications
{
    sealed class FCMPushNotifications<TUserId> : IPushNotifications<TUserId>
    {
        public Task SendToAllDevices(TUserId to, PushNotification notification)
        {
            throw new NotImplementedException();
        }

        public Task Send(TUserId to, DeviceType device, PushNotification notification)
        {
            throw new NotImplementedException();
        }

        public Task Send(TUserId to, FCMAndroidNotification notification)
        {
            throw new NotImplementedException();
        }

        public Task Send(TUserId to, FCMiOSNotification notification)
        {
            throw new NotImplementedException();
        }
    }

}
