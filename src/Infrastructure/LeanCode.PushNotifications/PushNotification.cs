namespace LeanCode.PushNotifications
{
    public sealed class PushNotification
    {
        public string Title { get; }
        public string Content { get; }
        public object? Data { get; }

        public PushNotification(string title, string content, object? data)
        {
            Title = title;
            Content = content;
            Data = data;
        }
    }
}
