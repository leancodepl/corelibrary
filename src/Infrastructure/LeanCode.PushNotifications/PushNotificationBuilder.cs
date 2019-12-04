using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static System.Globalization.CultureInfo;

namespace LeanCode.PushNotifications
{
    public sealed class PushNotificationBuilder<TUserId>
        where TUserId : notnull
    {
        private readonly IPushNotifications<TUserId> pushNotifications;

        [MaybeNull]
        public TUserId ToUser { get; private set; } = default!;

        public string? Title { get; private set; } = null;
        public string? Content { get; private set; } = null;
        public object? Data { get; private set; } = null;

        internal PushNotificationBuilder(IPushNotifications<TUserId> pushNotifications)
        {
            this.pushNotifications = pushNotifications;
        }

        public PushNotificationBuilder<TUserId> To(TUserId userId)
        {
            ToUser = userId;

            return this;
        }

        public PushNotificationBuilder<TUserId> WithTitle(string title)
        {
            Title = title;

            return this;
        }

        public PushNotificationBuilder<TUserId> WithTitle(string titleFormat, params object[] arguments)
        {
            Title = string.Format(InvariantCulture, titleFormat, arguments);

            return this;
        }

        public PushNotificationBuilder<TUserId> WithContent(string content)
        {
            Content = content;

            return this;
        }

        public PushNotificationBuilder<TUserId> WithContent(string contentFormat, params object[] arguments)
        {
            Content = string.Format(InvariantCulture, contentFormat, arguments);

            return this;
        }

        public PushNotificationBuilder<TUserId> WithData(object data)
        {
            Data = data;

            return this;
        }

        public Task SendToDeviceAsync(DeviceType device)
        {
            if (ToUser is null || ToUser.Equals(default))
            {
                throw new ArgumentException("Recipient was not specified.");
            }

            return pushNotifications.SendAsync(ToUser, device, new PushNotification(
                Title ?? string.Empty, Content ?? string.Empty, Data));
        }

        public Task SendToAllDevicesAsync()
        {
            if (ToUser is null || ToUser.Equals(default))
            {
                throw new ArgumentException("Recipient was not specified.");
            }

            return pushNotifications.SendToAllAsync(ToUser, new PushNotification(
                Title ?? string.Empty, Content ?? string.Empty, Data));
        }
    }
}
