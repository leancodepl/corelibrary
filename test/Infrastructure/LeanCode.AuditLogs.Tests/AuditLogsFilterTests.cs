using MassTransit;
using NSubstitute;
using Xunit;

namespace LeanCode.AuditLogs.Tests;

public sealed class AuditLogsFilterTests
{
    private const string ConsumerName = "Test.Consumer.Name";

    [Fact]
    public async void Extracts_changes_after_pipeline_execution()
    {
        using var dbContext = new TestDbContext();
        var publisher = Substitute.For<AuditLogsPublisher>();
        var filter = new AuditLogsFilter<TestDbContext, Consumer, TestMsg>(dbContext, publisher);

        var consumer = new Consumer();
        var context = Substitute.For<ConsumerConsumeContext<Consumer, TestMsg>>();
        context.Consumer.Returns(consumer);

        await filter.Send(context, Substitute.For<IPipe<ConsumerConsumeContext<Consumer, TestMsg>>>());

        await publisher
            .Received()
            .ExtractAndPublishAsync(dbContext, context, ConsumerName, Arg.Any<CancellationToken>());
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1034",
        Justification = "Should not be used outside the test"
    )]
    public sealed class TestMsg { }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1034",
        Justification = "Should not be used outside the test"
    )]
    public class Consumer : IConsumer<TestMsg>
    {
        public Task Consume(ConsumeContext<TestMsg> context) => Task.CompletedTask;

        public override string ToString() => ConsumerName;
    }
}
