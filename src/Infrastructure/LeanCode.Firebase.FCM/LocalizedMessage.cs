using System.Globalization;
using FirebaseAdmin.Messaging;
using LeanCode.Localization.StringLocalizers;

namespace LeanCode.Firebase.FCM
{
    public class LocalizedNotification
    {
        private readonly IStringLocalizer localizer;
        private readonly CultureInfo culture;

        private readonly Notification notification = new Notification();

        public LocalizedNotification(IStringLocalizer localizer, CultureInfo culture)
        {
            this.localizer = localizer;
            this.culture = culture;
        }

        public LocalizedNotification Title(string titleKey, params object[] arguments)
        {
            notification.Title = localizer.Format(culture, titleKey, arguments);
            return this;
        }

        public LocalizedNotification Body(string bodyKey, params object[] arguments)
        {
            notification.Body = localizer.Format(culture, bodyKey, arguments);
            return this;
        }

        public LocalizedNotification RawImageUrl(string rawUrl)
        {
            notification.ImageUrl = rawUrl;
            return this;
        }

        public LocalizedNotification ImageUrl(string urlKey, params object[] arguments)
        {
            notification.ImageUrl = localizer.Format(culture, urlKey, arguments);
            return this;
        }

        public Notification Build() => notification;

        public static implicit operator Notification(LocalizedNotification n) => n.Build();
    }
}
