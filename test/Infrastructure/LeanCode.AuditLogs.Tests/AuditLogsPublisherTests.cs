using MassTransit;
using NSubstitute;
using Xunit;

namespace LeanCode.AuditLogs.Tests;

public class AuditLogsPublisherTests
{
    private readonly TestDbContext dbContext;
    private readonly AuditLogsPublisher auditLogsPublisher;

    public AuditLogsPublisherTests()
    {
        dbContext = new TestDbContext();
        auditLogsPublisher = new AuditLogsPublisher();
    }

    [Fact]
    public async void Check_if_publisher_does_nothing_when_nothing_changed()
    {
        var bus = Substitute.For<IBus>();
        await auditLogsPublisher.ExtractAndPublishAsync(dbContext, bus, string.Empty, default);

        await bus.DidNotReceiveWithAnyArgs().Publish(default!, default!);
    }

    [Fact]
    public async void Check_if_publishes_change()
    {
        dbContext.TestEntities.Add(TestEntity.Create("id"));
        var bus = Substitute.For<IBus>();
        await auditLogsPublisher.ExtractAndPublishAsync(dbContext, bus, string.Empty, default);

        await bus.ReceivedWithAnyArgs(1).Publish((AuditLogMessage)default!, default!);
    }
}
