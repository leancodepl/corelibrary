namespace LeanCode.AppRating.DataAccess;

public sealed record class AppRatingEntity<TUserId>(
    TUserId UserId,
    DateTime DateCreated,
    double Rating,
    string? AdditionalComment,
    Platform Platform,
    string SystemVersion,
    string AppVersion,
    Dictionary<string, object>? Metadata
)
    where TUserId : notnull, IEquatable<TUserId>;

public enum Platform
{
    Android = 0,
    IOS = 1,
}
