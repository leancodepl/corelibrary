namespace LeanCode.TimeProvider;

public sealed class AsyncLocalTimeProvider : System.TimeProvider
{
    public static AsyncLocalTimeProvider Instance { get; } = new(UtcSystemTimeProvider.Instance);

    public AsyncLocal<System.TimeProvider?> Inner { get; private set; } = new();

    public System.TimeProvider Fallback { get; }

    public System.TimeProvider CurrentValue
    {
        get => Inner.Value ?? Fallback;
        set => Inner.Value = value;
    }

    public override TimeZoneInfo LocalTimeZone => CurrentValue.LocalTimeZone;

    public override long TimestampFrequency => CurrentValue.TimestampFrequency;

    private AsyncLocalTimeProvider(System.TimeProvider fallback)
    {
        Fallback = fallback;
    }

    public static void Activate(System.TimeProvider inner)
    {
        Instance.Inner.Value = inner;
        Time.Provider = Instance;
    }

    public static void Reset() => Instance.Inner = new();

    public override DateTimeOffset GetUtcNow() => CurrentValue.GetUtcNow();

    public override long GetTimestamp() => CurrentValue.GetTimestamp();

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) =>
        CurrentValue.CreateTimer(callback, state, dueTime, period);
}
