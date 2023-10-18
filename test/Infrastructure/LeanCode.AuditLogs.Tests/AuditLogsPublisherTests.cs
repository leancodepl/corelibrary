using MassTransit;
using NSubstitute;
using Xunit;

namespace LeanCode.AuditLogs.Tests;

public class AuditLogsPublisherTests
{
    private readonly TestDbContext dbContext;

    public AuditLogsPublisherTests()
    {
        dbContext = new TestDbContext();
    }

    [Fact]
    public async void Check_if_publisher_does_nothing_when_nothing_changed()
    {
        var bus = Substitute.For<IBus>();
        await AuditLogsPublisher.ExtractAndPublishAsync(dbContext, bus, string.Empty, default);

        await bus.DidNotReceiveWithAnyArgs().Publish(default!, default!);
    }

    [Fact]
    public async void Check_if_publishes_change()
    {
        dbContext.TestEntities.Add(TestEntity.Create("id"));
        var bus = Substitute.For<IBus>();
        await AuditLogsPublisher.ExtractAndPublishAsync(dbContext, bus, string.Empty, default);

        await bus.Received(1).Publish(default!, default!);
    }
}
