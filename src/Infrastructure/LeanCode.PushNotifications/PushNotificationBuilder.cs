using System;
using System.Globalization;
using System.Threading.Tasks;
using LeanCode.Localization.StringLocalizers;
using static System.Globalization.CultureInfo;

namespace LeanCode.PushNotifications
{
    public sealed class PushNotificationBuilder<TUserId>
    {
        private readonly IPushNotifications<TUserId> pushNotifications;

        public TUserId ToAddress { get; private set; }
        public string Title { get; private set; } = null;
        public string Content { get; private set; } = null;
        public object Data { get; private set; } = null;

        internal PushNotificationBuilder(IPushNotifications<TUserId> pushNotifications)
        {
            this.pushNotifications = pushNotifications ?? throw new ArgumentNullException(nameof(pushNotifications));
        }

        public PushNotificationBuilder<TUserId> To(TUserId userId)
        {
            ToAddress = userId;
            return this;
        }

        public PushNotificationBuilder<TUserId> WithTitle(string title)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            return this;
        }

        public PushNotificationBuilder<TUserId> WithTitle(
            string titleFormat, params object[] arguments)
        {
            _ = titleFormat ?? throw new ArgumentNullException(nameof(titleFormat));

            Title = string.Format(InvariantCulture, titleFormat, arguments);

            return this;
        }

        public PushNotificationBuilder<TUserId> WithContent(string content)
        {
            Content = content;
            return this;
        }

        public PushNotificationBuilder<TUserId> WithContent(
            string contentFormat, params object[] arguments)
        {
            _ = contentFormat ?? throw new ArgumentNullException(nameof(contentFormat));

            Content = string.Format(InvariantCulture, contentFormat, arguments);

            return this;
        }

        public PushNotificationBuilder<TUserId> WithData(object data)
        {
            Data = data;
            return this;
        }

        public Task SendToDeviceAsync(DeviceType device) =>
            pushNotifications.SendAsync(ToAddress, device, new PushNotification(Title, Content, Data));

        public Task SendToAllDevicesAsync() =>
            pushNotifications.SendToAllAsync(ToAddress, new PushNotification(Title, Content, Data));
    }
}
