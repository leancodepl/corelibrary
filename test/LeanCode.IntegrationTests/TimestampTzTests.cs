using FluentAssertions;
using LeanCode.DomainModels.Model;
using LeanCode.IntegrationTests.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.IntegrationTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1001", Justification = "Disposed with `IAsyncLifetime`.")]
public class TimestampTzTests : IAsyncLifetime
{
    private static readonly DateOnly Date = new(2023, 10, 5);

    private readonly TestApp app;

    private readonly Meeting meeting1 =
        new()
        {
            Id = Guid.NewGuid(),
            Name = "First",
            StartTime = new(Date.ToDateTime(new(10, 0), DateTimeKind.Utc), "Asia/Tokyo")
        };

    private readonly Meeting meeting2 =
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Second",
            StartTime = new(Date.ToDateTime(new(14, 0), DateTimeKind.Utc), "America/Los_Angeles")
        };

    private TestDbContext dbContext;

    public TimestampTzTests()
    {
        app = new TestApp();
        dbContext = null!;
    }

    [PostgresFact]
    public async Task Sorting_by_UtcTimestamp_returns_results_in_expected_order()
    {
        var orderedByUtc = await dbContext.Meetings.OrderBy(m => m.StartTime.UtcTimestamp).ToListAsync();

        orderedByUtc.Should().BeEquivalentTo([ meeting1, meeting2 ], options => options.WithStrictOrdering());
    }

    [PostgresFact]
    public async Task Sorting_by_LocalTimestampWithoutOffset_returns_results_in_expected_order()
    {
        var orderedByLocal = await dbContext.Meetings
            .OrderBy(m => m.StartTime.LocalTimestampWithoutOffset)
            .ToListAsync();

        orderedByLocal.Should().BeEquivalentTo([ meeting2, meeting1 ], options => options.WithStrictOrdering());
    }

    [PostgresFact]
    public void Sorting_by_LocalTimestampWithoutOffset_generates_SQL_with_expected_AT_TIME_ZONE_operator()
    {
        dbContext.Meetings
            .OrderBy(m => m.StartTime.LocalTimestampWithoutOffset)
            .ToQueryString()
            .Should()
            .ContainEquivalentOf(
                @$"ORDER BY m.""{nameof(Meeting.StartTime)}_{nameof(TimestampTz.UtcTimestamp)}"" AT TIME ZONE m.""{nameof(Meeting.StartTime)}_{nameof(TimestampTz.TimeZoneId)}"""
            );
    }

    public async Task InitializeAsync()
    {
        await app.InitializeAsync();

        dbContext = app.Services.GetRequiredService<TestDbContext>();

        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        dbContext.Meetings.Add(meeting1);
        dbContext.Meetings.Add(meeting2);

        await dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync() => app.DisposeAsync().AsTask();
}
