using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LeanCode.PushNotifications
{
    public class FCMNotification
    {
        [JsonPropertyName("to")]
        public string? To { get; set; }
        [JsonPropertyName("content_available")]
        public bool ContentAvailable { get; set; }
        [JsonPropertyName("priority")]
        public string Priority { get; set; } = string.Empty;
        [JsonPropertyName("time_to_live")]
        public int TimeToLive { get; set; }
        [JsonPropertyName("notification")]
        public FCMNotificationPayload? Notification { get; set; }

        /// <remarks>
        /// Android FCM client gives access to the data payload using <code>Map&lt;String, String&gt;</code>
        /// so we must conform to this (iOS is more permissive);
        /// </remarks>
        [JsonPropertyName("data")]
        public Dictionary<string, string?>? Data { get; set; }
    }

    public class FCMNotificationPayload
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
        [JsonPropertyName("sound")]
        public string? Sound { get; set; }
        [JsonPropertyName("icon")]
        public string? Icon { get; set; }
        [JsonPropertyName("badge")]
        public string? Badge { get; set; }
    }
}
