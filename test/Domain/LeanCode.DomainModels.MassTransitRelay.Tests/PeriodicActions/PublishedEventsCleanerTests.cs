using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using LeanCode.Time;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.PeriodicActions;

public class PublishedEventsCleanerTests : DbTestBase
{
    private static readonly DateTime Now = new(2021, 4, 13, 12, 0, 0);
    private readonly PublishedEventsCleaner cleaner;

    public PublishedEventsCleanerTests()
    {
        FixedTimeProvider.SetTo(Now);
        cleaner = new(DbContext);
    }

    [Fact]
    public async Task Removes_old_published_events()
    {
        var eventId = await PrepareRaisedEventAsync(Now.AddDays(-7), true);

        await cleaner.ExecuteAsync(default);

        await AssertDoesNotExistAsync(eventId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Does_not_remove_fresh_published_and_unpublished_events(bool wasPublished)
    {
        var eventId = await PrepareRaisedEventAsync(Now.AddMinutes(-5), wasPublished);

        await cleaner.ExecuteAsync(default);

        await AssertExistsAsync(eventId);
    }

    [Fact]
    public async Task Does_not_remove_old_unpublished_events()
    {
        var eventId = await PrepareRaisedEventAsync(Now.AddDays(-5), false);

        await cleaner.ExecuteAsync(default);

        await AssertExistsAsync(eventId);
    }

    private async Task AssertDoesNotExistAsync(Guid eventId)
    {
        var r = await DbContext.RaisedEvents.AnyAsync(e => e.Id == eventId);
        Assert.False(r);
    }

    private async Task AssertExistsAsync(Guid eventId)
    {
        var r = await DbContext.RaisedEvents.AnyAsync(e => e.Id == eventId);
        Assert.True(r);
    }

    private async Task<Guid> PrepareRaisedEventAsync(DateTime dateOccurred, bool wasPublished)
    {
        var evt = new RaisedEvent(Guid.NewGuid(), new(), dateOccurred, wasPublished, "TestEventType", "{}");

        DbContext.RaisedEvents.Add(evt);
        await DbContext.SaveChangesAsync();

        return evt.Id;
    }
}
