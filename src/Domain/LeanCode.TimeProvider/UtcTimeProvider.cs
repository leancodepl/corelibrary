namespace LeanCode.TimeProvider;

public sealed class UtcTimeProvider : ITimeProvider
{
    public DateTime Now => DateTime.UtcNow;
    public DateTimeOffset NowWithOffset => DateTimeOffset.UtcNow;
}
