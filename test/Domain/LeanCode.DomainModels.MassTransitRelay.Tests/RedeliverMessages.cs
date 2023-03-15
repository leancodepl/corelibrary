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

        harness.OnConfigureInMemoryBus += configuration =>
        {
            configuration.UseDelayedMessageScheduler();
        };

        var consumerHarness = harness.Consumer<MyConsumer>();

        await harness.Start();

        try
        {
            var message = new MyMessage { Text = "test" };
            await harness.InputQueueSendEndpoint.Send(message);

            Assert.True(await consumerHarness.Consumed.SelectAsync<MyMessage>().Any());

            var context = consumerHarness.Consumed.Select<MyMessage>().LastOrDefault()?.Context;

            var redeliveryCount = context.GetRedeliveryCount();
            var payload = context.GetPayload<ConsumeContext>();

            Assert.NotNull(payload);
            Assert.Equal(0, redeliveryCount);

            await context.Redeliver(TimeSpan.FromSeconds(1));
            await Task.Delay(TimeSpan.FromSeconds(1));

            context = consumerHarness.Consumed.Select<MyMessage>().LastOrDefault().Context;
            redeliveryCount = context.GetRedeliveryCount();

            Assert.Equal(2, consumerHarness.Consumed.Select<MyMessage>().Count());
            Assert.Equal(1, redeliveryCount);
        }
        finally
        {
            await harness.Stop();
        }
    }

    private class MyConsumer : IConsumer<MyMessage>
    {
        public async Task Consume(ConsumeContext<MyMessage> context)
        {
            await Task.CompletedTask;
        }
    }

    private class MyMessage
    {
        public string Text { get; set; }
    }
}
