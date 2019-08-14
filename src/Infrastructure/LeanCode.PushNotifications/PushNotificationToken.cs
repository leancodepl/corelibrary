using System;
using System.Diagnostics.CodeAnalysis;

namespace LeanCode.PushNotifications
{
    public enum DeviceType
    {
        Android = 0,
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Special-case for platform name.")]
        iOS = 1,
        Chrome = 2,
    }

    public class PushNotificationToken<TUserId>
    {
        public Guid Id { get; }
        public TUserId UserId { get; }
        public DeviceType DeviceType { get; }
        public string Token { get; }

        public PushNotificationToken(Guid id, TUserId userId, DeviceType deviceType, string token)
        {
            Id = id;
            UserId = userId;
            DeviceType = deviceType;
            Token = token;
        }
    }
}
