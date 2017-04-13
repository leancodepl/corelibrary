using Xunit;

namespace LeanCode.PushNotifications.Tests
{
    public class NotificationTransformerAndroidTests
    {
        [Fact]
        public void Sets_basic_fields_to_corresponding_values()
        {
            var result = NotificationTransformer.ConvertToAndroid(new PushNotification(
                title: "",
                content: "",
                data: null
            ));
        }
    }
}
