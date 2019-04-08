using System;
using System.Globalization;
using System.Threading.Tasks;
using LeanCode.Localization.StringLocalizers;
using static System.Globalization.CultureInfo;

namespace LeanCode.PushNotifications
{
    public sealed class LocalizedPushNotificationBuilder<TUserId>
    {
        private readonly CultureInfo culture;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IPushNotifications<TUserId> pushNotifications;

        public TUserId ToAddress { get; private set; }
        public string Title { get; private set; } = null;
        public string Content { get; private set; } = null;
        public object Data { get; private set; } = null;

        internal LocalizedPushNotificationBuilder(
            string cultureName,
            IStringLocalizer stringLocalizer,
            IPushNotifications<TUserId> pushNotifications)
        {
            _ = cultureName ?? throw new ArgumentNullException(nameof(cultureName));

            this.pushNotifications = pushNotifications ?? throw new ArgumentNullException(nameof(pushNotifications));
            this.stringLocalizer = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
            this.culture = GetCultureInfo(cultureName);
        }

        public LocalizedPushNotificationBuilder<TUserId> To(TUserId userId)
        {
            ToAddress = userId;
            return this;
        }

        public LocalizedPushNotificationBuilder<TUserId> WithTitle(string titleKey)
        {
            _ = titleKey ?? throw new ArgumentNullException(nameof(titleKey));
            Title = stringLocalizer[culture, titleKey];

            return this;
        }

        public LocalizedPushNotificationBuilder<TUserId> WithTitle(
            string titleFormatKey, params object[] arguments)
        {
            _ = titleFormatKey ?? throw new ArgumentNullException(nameof(titleFormatKey));

            string format = stringLocalizer[culture, titleFormatKey];
            Title = string.Format(culture, format, arguments);

            return this;
        }

        public LocalizedPushNotificationBuilder<TUserId> WithContent(string contentKey)
        {
            if (contentKey is null)
            {
                Content = null;
            }
            else
            {
                Content = stringLocalizer[culture, contentKey];
            }

            return this;
        }

        public LocalizedPushNotificationBuilder<TUserId> WithContent(
            string contentFormatKey, params object[] arguments)
        {
            _ = contentFormatKey ?? throw new ArgumentNullException(nameof(contentFormatKey));

            string format = stringLocalizer[culture, contentFormatKey];
            Content = string.Format(culture, format, arguments);

            return this;
        }

        public LocalizedPushNotificationBuilder<TUserId> WithData(object data)
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
