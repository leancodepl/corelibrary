namespace LeanCode.TimeProvider;

public static class Time
{
    public static System.TimeProvider Provider { get; set; } = UtcSystemTimeProvider.Instance;

    public static DateTime Now => NowWithOffset.DateTime;
    public static DateTimeOffset NowWithOffset => Provider.GetLocalNow();
}
