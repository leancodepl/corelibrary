namespace LeanCode.TimeProvider;

public interface ITimeProvider
{
    DateTime Now { get; }
    DateTimeOffset NowWithOffset { get; }
}
