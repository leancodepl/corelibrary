namespace LeanCode.TimeProvider;

public static class Time
{
    private static ITimeProvider timeProvider = new UtcTimeProvider();

    public static DateTime Now => timeProvider.Now;
    public static DateTimeOffset NowWithOffset => timeProvider.NowWithOffset;

    public static void Use(ITimeProvider newProvider) => timeProvider = newProvider;
}
