using System;
using Xunit;

namespace LeanCode.PushNotifications.Tests
{
    public abstract class BaseNotificationTransformerTests
    {
        protected abstract Func<PushNotification, FCMNotification> Convert { get; }

        [Fact]
        public void Sets_basic_fields_to_corresponding_values()
        {
            var notification = new PushNotification(
                title: string.Empty,
                content: string.Empty,
                data: null);
            var result = Convert(notification);

            Assert.Null(result.To);
            Assert.True(result.ContentAvailable);
            Assert.Equal("high", result.Priority);
            Assert.Equal(28 * 24 * 60 * 60, result.TimeToLive); // 28 days, max value
        }

        [Fact]
        public void Sets_notification_payload_fields()
        {
            var notification = new PushNotification(
                title: "The title",
                content: "The content",
                data: null);
            var result = Convert(notification);

            Assert.NotNull(result.Notification);
            Assert.Equal(notification.Title, result.Notification.Title);
            Assert.Equal(notification.Content, result.Notification.Body);
        }

        [Fact]
        public void Stores_the_data_payload_type_in_the_dictionary()
        {
            var data = new SampleData();
            var notification = new PushNotification(string.Empty, string.Empty, data);

            var result = Convert(notification);

            Assert.NotNull(result.Data);
            AssertElement(result, "Type", data.GetType().Name);
        }

        [Fact]
        public void Stores_ToStrings_of_the_properties_in_the_data_payload()
        {
            var data = new SampleData { IntProp = 3, StringProp = "abc", InnerProp = new SampleData() };
            var notification = new PushNotification(string.Empty, string.Empty, data);

            var result = Convert(notification);

            AssertElement(result, nameof(data.IntProp), data.IntProp.ToString());
            AssertElement(result, nameof(data.StringProp), data.StringProp);
            AssertElement(result, nameof(data.InnerProp), data.InnerProp.ToString());
        }

        [Fact]
        public void Sets_data_payload_to_null_if_null_was_used()
        {
            var notification = new PushNotification(string.Empty, string.Empty, null);

            var result = Convert(notification);

            Assert.Null(result.Data);
        }

        [Fact]
        public void Skips_null_properties_on_data_payload()
        {
            var data = new SampleData { StringProp = null };
            var notification = new PushNotification(string.Empty, string.Empty, data);

            var result = Convert(notification);

            Assert.False(result.Data.ContainsKey(nameof(data.StringProp)));
        }

        [Fact]
        public void Skips_fields_that_are_named_Type()
        {
            var data = new SampleDataType { Type = "some type" };
            var notification = new PushNotification(string.Empty, string.Empty, data);

            var result = Convert(notification);

            AssertElement(result, "Type", data.GetType().Name);
        }

        private static void AssertElement(FCMNotification notification, string key, string expectedValue)
        {
            Assert.True(notification.Data.TryGetValue(key, out var value));
            Assert.Equal(expectedValue, value);
        }
    }
}
