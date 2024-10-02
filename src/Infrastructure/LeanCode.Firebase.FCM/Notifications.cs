namespace LeanCode.Firebase.FCM;

public static class Notifications
{
    private static readonly NotificationDataConverter SharedConverter = NotificationDataConverter.New().Build();

    /// <summary>
    /// Converts POCO object to a notification data dictionary. Does not support hierarchical
    /// data.
    /// </summary>
    [Obsolete("Use `NotificationDataConverter.ToNotificationData` instead` ")]
    public static Dictionary<string, string> ToNotificationData(object data)
    {
        return SharedConverter.ToNotificationData(data);
    }
}
