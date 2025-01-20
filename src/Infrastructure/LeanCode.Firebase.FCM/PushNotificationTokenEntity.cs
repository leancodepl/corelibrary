namespace LeanCode.Firebase.FCM;

public sealed record PushNotificationTokenEntity<TUserId>(TUserId UserId, string Token, DateTime DateCreated)
    where TUserId : IEquatable<TUserId>;
