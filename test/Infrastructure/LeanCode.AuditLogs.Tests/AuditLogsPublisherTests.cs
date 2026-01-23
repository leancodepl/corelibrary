using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using MassTransit;
using NSubstitute;
using Xunit;

namespace LeanCode.AuditLogs.Tests;

public class AuditLogsPublisherTests : IDisposable
{
    private readonly TestDbContext dbContext;
    private readonly AuditLogsPublisher auditLogsPublisher;

    public AuditLogsPublisherTests()
    {
        dbContext = new TestDbContext();
        auditLogsPublisher = new AuditLogsPublisher();
    }

    [Fact]
    public async Task Check_if_publisher_does_nothing_when_nothing_changed()
    {
        var bus = Substitute.For<IBus>();
        await auditLogsPublisher.ExtractAndPublishAsync(dbContext, bus, string.Empty, default);

        await bus.DidNotReceiveWithAnyArgs().Publish(default!);
    }

    [Fact]
    public async Task Check_if_publishes_change()
    {
        dbContext.TestEntities.Add(TestEntity.Create("id"));
        var bus = Substitute.For<IBus>();
        await auditLogsPublisher.ExtractAndPublishAsync(dbContext, bus, string.Empty, default);

        await bus.ReceivedWithAnyArgs(1).Publish((AuditLogMessage)default!);
    }

    [Fact]
    public async Task Check_if_publishes_change_with_custom_json_options()
    {
        var customDbContext = new TestDbContext();
        var publisherWithOptions = new AuditLogsPublisher(
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = false,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            }
        );
        customDbContext.TestEntities.Add(TestEntity.Create("id"));
        var bus = Substitute.For<IBus>();
        await publisherWithOptions.ExtractAndPublishAsync(customDbContext, bus, string.Empty, default);

        await bus.ReceivedWithAnyArgs(1).Publish((AuditLogMessage)default!);
        await customDbContext.DisposeAsync();
    }

    public void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        dbContext.Dispose();
    }
}
