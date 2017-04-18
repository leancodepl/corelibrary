using System;

namespace LeanCode.PushNotifications
{
    public enum DeviceType
    {
        Android = 0,
        iOS = 1,
        Chrome = 2
    }

    public class PushNotificationToken<TUserId>
    {
        public TUserId UserId { get; set; }
        public DeviceType DeviceType { get; set; }

        public string Token { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
