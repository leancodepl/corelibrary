using System;
using Xunit;

namespace LeanCode.PushNotifications.Tests
{
    public class NotificationTransformerAndroidTests : BaseNotificationTransformerTests
    {
        protected override Func<PushNotification, FCMNotification> Convert { get; }
            = NotificationTransformer.ConvertToAndroid;

        [Fact]
        public void Sets_Android_specific_notification_payload_fields()
        {
            var notification = new PushNotification(
                title: "The title",
                content: "The content",
                data: null);
            var result = Convert(notification);

            Assert.Null(result.Notification.Icon);
            Assert.Equal("default", result.Notification.Sound);
            Assert.Null(result.Notification.Badge);
        }
    }
}
