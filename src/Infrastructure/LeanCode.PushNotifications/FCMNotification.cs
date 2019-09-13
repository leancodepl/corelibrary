using System.Collections.Generic;
using Newtonsoft.Json;

namespace LeanCode.PushNotifications
{
    public class FCMNotification
    {
        [JsonProperty("to")]
        public string? To { get; set; }
        [JsonProperty("content_available")]
        public bool ContentAvailable { get; set; }
        [JsonProperty("priority")]
        public string Priority { get; set; } = string.Empty;
        [JsonProperty("time_to_live")]
        public int TimeToLive { get; set; }
        [JsonProperty("notification")]
        public FCMNotificationPayload? Notification { get; set; }

        /// <remarks>
        /// Android FCM client gives access to the data payload using <code>Map&lt;String, String&gt;</code>
        /// so we must conform to this (iOS is more permissive);
        /// </remarks>
        [JsonProperty("data")]
        public Dictionary<string, string?>? Data { get; set; }
    }

    public class FCMNotificationPayload
    {
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;
        [JsonProperty("body")]
        public string Body { get; set; } = string.Empty;
        [JsonProperty("sound")]
        public string? Sound { get; set; }
        [JsonProperty("icon")]
        public string? Icon { get; set; }
        [JsonProperty("badge")]
        public string? Badge { get; set; }
    }
}
