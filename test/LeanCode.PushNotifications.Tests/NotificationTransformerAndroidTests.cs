using System;
using Xunit;

namespace LeanCode.PushNotifications.Tests
{
    public class NotificationTransformerAndroidTests : BaseNotificationTransformerTests<FCMAndroidNotificationPayload>
    {
        protected override Func<PushNotification, FCMNotification<FCMAndroidNotificationPayload>> Convert { get; }
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
        }
    }
}
