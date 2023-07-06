using LeanCode.Test.Helpers;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace LeanCode.TimeProvider.Tests;

public abstract class TimeProviderTests
{
    private const int Iterations = 10_000;

    private const int DelayIterations = 100;
    private const int Delay = 10;

    private readonly DateTimeOffset expectedTime;
    private readonly TimeZoneInfo timeZoneInfo;

    protected TimeProviderTests(DateTimeOffset expectedTime, TimeZoneInfo timeZoneInfo = null)
    {
        this.expectedTime = expectedTime;
        this.timeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Utc;
    }

    [LongRunningFact]
    public void Check_the_value_sync()
    {
        SetTime();

        for (var i = 0; i < Iterations; i++)
        {
            var now = Time.NowWithOffset;
            Assert.Equal(expectedTime, now);
            Assert.Equal(expectedTime.Offset, now.Offset);
        }
    }

    [LongRunningFact]
    public async Task Check_the_value_async()
    {
        SetTime();

        for (var i = 0; i < DelayIterations; i++)
        {
            var now = Time.NowWithOffset;
            Assert.Equal(expectedTime, now);
            Assert.Equal(expectedTime.Offset, now.Offset);

            await Task.Delay(Delay);
        }
    }

    [Fact]
    public void TimeNow_returns_timestamp_in_providers_time_zone()
    {
        SetTime();

        var now = Time.Now;
        Assert.Equal(expectedTime.DateTime, now);
        Assert.Equal(DateTimeKind.Unspecified, now.Kind);
    }

    private void SetTime()
    {
        var provider = new FakeTimeProvider(expectedTime.ToUniversalTime());
        provider.SetLocalTimeZone(timeZoneInfo);
        AsyncLocalTimeProvider.Activate(provider);
    }
}

public class TimeProviderTests1 : TimeProviderTests
{
    public TimeProviderTests1()
        : base(new DateTimeOffset(new DateTime(2017, 11, 30, 15, 45, 0, DateTimeKind.Utc))) { }
}

public class TimeProviderTests2 : TimeProviderTests
{
    public TimeProviderTests2()
        : base(new DateTimeOffset(new DateTime(2016, 11, 30, 0, 0, 0, DateTimeKind.Utc))) { }
}

public class TimeProviderTests3 : TimeProviderTests
{
    public TimeProviderTests3()
        : base(
            new DateTimeOffset(new DateTime(2015, 11, 30, 6, 0, 0), TimeSpan.FromHours(9d)),
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo")
        ) { }
}
