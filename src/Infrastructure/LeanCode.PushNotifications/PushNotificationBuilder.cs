using System;
using System.Globalization;
using System.Threading.Tasks;
using LeanCode.Localization.StringLocalizers;
using static System.Globalization.CultureInfo;

namespace LeanCode.PushNotifications
{
    public class PushNotificationBuilder<TUserId>
    {
        private readonly CultureInfo culture;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IPushNotifications<TUserId> pushNotifications;

        public TUserId To { get; private set; }
        public string Title { get; private set; } = null;
        public string Content { get; private set; } = null;
        public object Data { get; private set; } = null;

        internal PushNotificationBuilder(
            IPushNotifications<TUserId> pushNotifications,
            TUserId to)
        {
            this.pushNotifications = pushNotifications ?? throw new ArgumentNullException(nameof(pushNotifications));
            this.stringLocalizer = null;
            this.culture = null;
            To = to;
        }

        internal PushNotificationBuilder(
            string cultureName,
            IStringLocalizer stringLocalizer,
            IPushNotifications<TUserId> pushNotifications,
            TUserId to)
        {
            _ = cultureName ?? throw new ArgumentNullException(nameof(cultureName));

            this.pushNotifications = pushNotifications ?? throw new ArgumentNullException(nameof(pushNotifications));
            this.stringLocalizer = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
            this.culture = GetCultureInfo(cultureName);
            To = to;
        }

        public PushNotificationBuilder<TUserId> WithTitle(string title)
        {
            if (culture is null || title is null)
            {
                Title = title;
            }
            else
            {
                Title = stringLocalizer[culture, title];
            }

            return this;
        }

        public PushNotificationBuilder<TUserId> WithTitle(string title, params object[] arguments)
        {
            if (culture is null)
            {
                Title = string.Format(title, arguments);
            }
            else
            {
                string format = stringLocalizer[culture, title];
                Title = string.Format(culture, format, arguments);
            }

            return this;
        }

        public PushNotificationBuilder<TUserId> WithContent(string content)
        {
            if (culture is null || content is null)
            {
                Content = content;
            }
            else
            {
                Content = stringLocalizer[culture, content];
            }

            return this;
        }

        public PushNotificationBuilder<TUserId> WithContent(string content, params object[] arguments)
        {
            if (culture is null)
            {
                Content = string.Format(content, arguments);
            }
            else
            {
                string format = stringLocalizer[culture, content];
                Content = string.Format(culture, format, arguments);
            }

            return this;
        }

        public PushNotificationBuilder<TUserId> WithData(object data)
        {
            Data = data;
            return this;
        }

        public Task SendToDevice(DeviceType device) =>
            pushNotifications.Send(To, device, new PushNotification(Title, Content, Data));

        public Task SendToAllDevices() =>
            pushNotifications.SendToAll(To, new PushNotification(Title, Content, Data));
    }
}
