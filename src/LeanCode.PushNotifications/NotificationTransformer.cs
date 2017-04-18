using System.Collections.Generic;
using System.Reflection;

namespace LeanCode.PushNotifications
{
    static class NotificationTransformer
    {
        const string Priority = "high";
        const int TTL = 28 * 24 * 60 * 60;
        const string TypeField = "Type";

        public static FCMNotification ConvertToAndroid(PushNotification notification)
        {
            return new FCMNotification
            {
                To = null,
                ContentAvailable = true,
                Priority = "high",
                TimeToLive = TTL,
                Notification = new FCMNotificationPayload
                {
                    Title = notification.Title,
                    Body = notification.Content,
                    Sound = "default",
                    Icon = null,
                    Badge = null
                },
                Data = ConvertData(notification.Data)
            };
        }

        public static FCMNotification ConvertToiOS(PushNotification notification)
        {
            return new FCMNotification
            {
                To = null,
                ContentAvailable = true,
                Priority = "high",
                TimeToLive = TTL,
                Notification = new FCMNotificationPayload
                {
                    Title = notification.Title,
                    Body = notification.Content,
                    Sound = null,
                    Icon = null,
                    Badge = "1"
                },
                Data = ConvertData(notification.Data)
            };
        }

        public static FCMNotification ConvertToChrome(PushNotification notification)
        {
            return new FCMNotification
            {
                To = null,
                ContentAvailable = true,
                Priority = "high",
                TimeToLive = TTL,
                Notification = new FCMNotificationPayload
                {
                    Title = notification.Title,
                    Body = notification.Content,
                    Sound = null,
                    Icon = null,
                    Badge = null
                },
                Data = ConvertData(notification.Data)
            };
        }

        private static Dictionary<string, string> ConvertData(object data)
        {
            if (data == null)
            {
                return null;
            }

            var type = data.GetType();

            var result = new Dictionary<string, string>();
            result.Add(TypeField, type.Name);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (prop.Name != TypeField)
                {
                    var value = prop.GetValue(data);
                    if (value != null)
                    {
                        result.Add(prop.Name, value.ToString());
                    }
                }
            }

            return result;
        }
    }
}
