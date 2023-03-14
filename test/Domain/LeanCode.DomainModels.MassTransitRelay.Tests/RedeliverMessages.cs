using LeanCode.DomainModels.MassTransitRelay.Inbox;
using MassTransit;
using MassTransit.Testing;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests;

public class RedeliverMessages
{
    [Fact]
    public async Task Redelivered_messages_are_not_filtered_by_ConsumeMessageOnceFilter()
    {
        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer<MyConsumer>();

        await harness.Start();

        try
        {
            var message = new MyMessage { Text = "test" };
            await harness.InputQueueSendEndpoint.Send(message);

            Assert.True(consumerHarness.Consumed.Select<MyMessage>().Any());

            var context = consumerHarness.Consumed.Select<MyMessage>().Last().Context;

            var redeliveryCount = context.GetRedeliveryCount();
            var payload = context.GetPayload<ConsumeContext>();

            Assert.NotNull(payload);

            await context.Redeliver(TimeSpan.FromSeconds(1));

            Assert.Equal(2, consumerHarness.Consumed.Select<MyMessage>().Count());
        }
        finally
        {
            await harness.Stop();
        }
    }

    public class MyConsumer : IConsumer<MyMessage>
    {
        public async Task Consume(ConsumeContext<MyMessage> context)
        {
            throw new Exception("Something went wrong!");
        }
    }

    public class MyMessage
    {
        public string Text { get; set; }
    }
}
