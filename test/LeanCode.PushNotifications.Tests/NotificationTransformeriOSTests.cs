using System;
using Xunit;

namespace LeanCode.PushNotifications.Tests
{
    public class NotificationTransformeriOSTests : BaseNotificationTransformerTests<FCMiOSNotificationPayload>
    {
        protected override Func<PushNotification, FCMNotification<FCMiOSNotificationPayload>> Convert { get; }
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
        }
    }
}
