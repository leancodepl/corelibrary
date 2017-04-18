using System.Collections.Generic;
using Newtonsoft.Json;

namespace LeanCode.PushNotifications
{
    public abstract class FCMNotification<TNotification>
        where TNotification : FCMNotificationPayload
    {
        [JsonProperty("to")]
        public string To { get; set; }
        [JsonProperty("content_available")]
        public bool ContentAvailable { get; set; }
        [JsonProperty("priority")]
        public string Priority { get; set; }
        [JsonProperty("time_to_live")]
        public int TimeToLive { get; set; }
        [JsonProperty("notification")]
        public TNotification Notification { get; set; }

        /// <remarks>
        /// Android FCM client gives access to the data payload using <code>Map&lt;String, String&gt;</code>
        /// so we must conform to this (iOS is more permissive);
        /// </remarks>
        [JsonProperty("data")]
        public Dictionary<string, string> Data { get; set; }
    }

    public class FCMAndroidNotification : FCMNotification<FCMAndroidNotificationPayload>
    { }

    public class FCMiOSNotification : FCMNotification<FCMiOSNotificationPayload>
    { }

    public class FCMNotificationPayload
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
    }

    public class FCMAndroidNotificationPayload : FCMNotificationPayload
    {
        [JsonProperty("sound")]
        public string Sound { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }

    public class FCMiOSNotificationPayload : FCMNotificationPayload
    {
        [JsonProperty("sound")]
        public string Sound { get; set; }
        [JsonProperty("badge")]
        public string Badge { get; set; }
    }
}
