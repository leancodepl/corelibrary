using System;
using System.Collections.Generic;
using System.Reflection;

namespace LeanCode.PushNotifications
{
    static class NotificationTransformer
    {
        const string Priority = "high";
        const int TTL = 28 * 24 * 60 * 60;
        const string TypeField = "Type";

        public static FCMNotification Convert(DeviceType deviceType, PushNotification notification, PushNotificationsConfiguration configuration = null)
        {
            switch (deviceType)
            {
                case DeviceType.Android:
                    return ConvertToAndroid(notification);
                case DeviceType.iOS:
                    return ConvertToiOS(notification);
                case DeviceType.Chrome:
                    return ConvertToChrome(notification, configuration);
            }
            throw new ArgumentException("Unknown device type.", nameof(deviceType));
        }

        public static FCMNotification ConvertToAndroid(PushNotification notification)
        {
            return new FCMNotification
            {
                To = null,
                ContentAvailable = true,
                Priority = Priority,
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
                Priority = Priority,
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
            return ConvertToChrome(notification, null);
        }

        public static FCMNotification ConvertToChrome(PushNotification notification, PushNotificationsConfiguration configuration)
        {
            if (configuration?.UseDataInsteadOfNotification == true)
            {
                var data = ConvertData(notification.Data);
                data.Add("Title", notification.Title);
                data.Add("Content", notification.Content);

                return new FCMNotification
                {
                    To = null,
                    ContentAvailable = true,
                    Priority = Priority,
                    TimeToLive = TTL,
                    Data = data
                };
            }

            return new FCMNotification
            {
                To = null,
                ContentAvailable = true,
                Priority = Priority,
                TimeToLive = TTL,
                Notification = new FCMNotificationPayload
                {
                    Title = notification.Title,
                    Body = notification.Content,
                    Sound = null,
                    Icon = configuration?.Icon,
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

            var result = new Dictionary<string, string>
            {
                [TypeField] = type.Name
            };

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
