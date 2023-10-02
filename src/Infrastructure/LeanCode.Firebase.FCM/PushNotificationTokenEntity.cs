using LeanCode.TimeProvider;

namespace LeanCode.Firebase.FCM;

public sealed record class PushNotificationTokenEntity<TUserId>(TUserId UserId, string Token, DateTime DateCreated)
    where TUserId : notnull, IEquatable<TUserId>;
