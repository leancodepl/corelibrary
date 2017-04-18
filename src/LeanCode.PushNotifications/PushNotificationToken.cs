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
        public TUserId UserId { get; }
        public DeviceType DeviceType { get; }
        public string Token { get; }

        public PushNotificationToken(TUserId userId, DeviceType deviceType, string token)
        {
            UserId = userId;
            DeviceType = deviceType;
            Token = token;
        }
    }
}
