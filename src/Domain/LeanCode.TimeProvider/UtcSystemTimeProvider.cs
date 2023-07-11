namespace LeanCode.TimeProvider;

public sealed class UtcSystemTimeProvider : System.TimeProvider
{
    public static UtcSystemTimeProvider Instance { get; } = new();

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
