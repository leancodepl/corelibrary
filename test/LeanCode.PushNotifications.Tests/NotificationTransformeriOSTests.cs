using System;
using Xunit;

namespace LeanCode.PushNotifications.Tests
{
    public class NotificationTransformeriOSTests : BaseNotificationTransformerTests
    {
        protected override Func<PushNotification, FCMNotification> Convert { get; }
            = NotificationTransformer.ConvertToiOS;

        [Fact]
        public void Sets_iOS_specific_notification_payload_fields()
        {
            var notification = new PushNotification(
                title: "The title",
                content: "The content",
                data: null);
            var result = Convert(notification);

            Assert.Equal("1", result.Notification.Badge);
            Assert.Null(result.Notification.Sound);
            Assert.Null(result.Notification.Icon);
        }
    }

    public class NotificationTransformerChromeTests : BaseNotificationTransformerTests
    {
        protected override Func<PushNotification, FCMNotification> Convert { get; }
            = NotificationTransformer.ConvertToChrome;

        [Fact]
        public void Sets_Chrome_specific_notification_payload_fields()
        {
            var notification = new PushNotification(
                title: "The title",
                content: "The content",
                data: null);
            var result = Convert(notification);

            Assert.Null(result.Notification.Badge);
            Assert.Null(result.Notification.Sound);
            Assert.Null(result.Notification.Icon);
        }
    }
}
