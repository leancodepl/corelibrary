using System;
using System.Globalization;
using System.Threading.Tasks;
using LeanCode.Localization.StringLocalizers;
using static System.Globalization.CultureInfo;

namespace LeanCode.PushNotifications
{
    public class PushNotificationBuilder<TUserId>
    {
        private readonly IPushNotifications<TUserId> pushNotifications;
        private readonly IStringLocalizer stringLocalizer;
        private readonly CultureInfo culture;

        public TUserId To { get; private set; }
        public string Title { get; private set; } = null;
        public string Content { get; private set; } = null;
        public object Data { get; private set; } = null;

        internal PushNotificationBuilder(
            IPushNotifications<TUserId> pushNotifications,
            IStringLocalizer stringLocalizer,
            TUserId to)
        {
            this.pushNotifications = pushNotifications ?? throw new ArgumentNullException(nameof(pushNotifications));
            this.stringLocalizer = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
            this.culture = null;
            To = to;
        }

        internal PushNotificationBuilder(
            IPushNotifications<TUserId> pushNotifications,
            IStringLocalizer stringLocalizer,
            string cultureName,
            TUserId to)
        {
            this.pushNotifications = pushNotifications ?? throw new ArgumentNullException(nameof(pushNotifications));
            this.stringLocalizer = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
            this.culture = GetCultureInfo(cultureName ?? throw new ArgumentNullException(nameof(cultureName)));
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
                Title = stringLocalizer[culture, title, arguments];
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
                Content = stringLocalizer[culture, content, arguments];
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
