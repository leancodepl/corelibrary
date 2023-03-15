using MassTransit;
using MassTransit.Testing;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests;

public class RedeliverMessages
{
    [Fact]
    public async Task Redelivered_messages_are_not_filtered_by_ConsumeMessageOnceFilter()
    {
        using (InMemoryTestHarness harness = new InMemoryTestHarness())
        {
            harness.OnConfigureInMemoryBus += configuration =>
            {
                configuration.UseDelayedMessageScheduler();
            };

            var consumerHarness = harness.Consumer<MyConsumer>();

            await harness.Start();

            var message = new MyMessage { Text = "test" };
            await harness.InputQueueSendEndpoint.Send(message);

            Assert.True(await consumerHarness.Consumed.SelectAsync<MyMessage>().Any());

            var messages = await consumerHarness.Consumed.SelectAsync<MyMessage>().ToListAsync();
            var context = messages.LastOrDefault()?.Context;

            var redeliveryCount = context.GetRedeliveryCount();
            var payload = context.GetPayload<ConsumeContext>();

            Assert.NotNull(payload);
            Assert.Equal(0, redeliveryCount);

            await context.Redeliver(TimeSpan.FromSeconds(1));
            await Task.Delay(TimeSpan.FromSeconds(1));

            messages = await consumerHarness.Consumed.SelectAsync<MyMessage>().ToListAsync();
            context = messages.LastOrDefault()?.Context;
            redeliveryCount = context.GetRedeliveryCount();

            Assert.Equal(2, await consumerHarness.Consumed.SelectAsync<MyMessage>().Count());
            Assert.Equal(1, redeliveryCount);
        }
    }

    private sealed class MyConsumer : IConsumer<MyMessage>
    {
        public async Task Consume(ConsumeContext<MyMessage> context)
        {
            await Task.CompletedTask;
        }
    }

    private sealed class MyMessage
    {
        public string Text { get; set; }
    }
}
