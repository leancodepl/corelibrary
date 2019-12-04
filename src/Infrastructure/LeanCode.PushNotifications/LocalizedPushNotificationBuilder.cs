using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using LeanCode.Localization.StringLocalizers;
using static System.Globalization.CultureInfo;

namespace LeanCode.PushNotifications
{
    public sealed class LocalizedPushNotificationBuilder<TUserId>
        where TUserId : notnull
    {
        private readonly CultureInfo culture;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IPushNotifications<TUserId> pushNotifications;

        [MaybeNull]
        public TUserId ToUser { get; private set; } = default!;

        public string? Title { get; private set; } = null;
        public string? Content { get; private set; } = null;
        public object? Data { get; private set; } = null;

        internal LocalizedPushNotificationBuilder(
            string cultureName,
            IStringLocalizer stringLocalizer,
            IPushNotifications<TUserId> pushNotifications)
        {
            this.pushNotifications = pushNotifications;
            this.stringLocalizer = stringLocalizer;
            culture = GetCultureInfo(cultureName);
        }

        public LocalizedPushNotificationBuilder<TUserId> To(TUserId userId)
        {
            ToUser = userId;

            return this;
        }

        public LocalizedPushNotificationBuilder<TUserId> WithTitle(string titleKey)
        {
            Title = stringLocalizer[culture, titleKey];

            return this;
        }

        public LocalizedPushNotificationBuilder<TUserId> WithTitle(string titleFormatKey, params object[] arguments)
        {
            Title = string.Format(culture, stringLocalizer[culture, titleFormatKey], arguments);

            return this;
        }

        public LocalizedPushNotificationBuilder<TUserId> WithContent(string? contentKey)
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

        public LocalizedPushNotificationBuilder<TUserId> WithContent(string contentFormatKey, params object[] arguments)
        {
            Content = string.Format(culture, stringLocalizer[culture, contentFormatKey], arguments);

            return this;
        }

        public LocalizedPushNotificationBuilder<TUserId> WithData(object data)
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
