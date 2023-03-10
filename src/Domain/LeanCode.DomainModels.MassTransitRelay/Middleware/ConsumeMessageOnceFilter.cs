using LeanCode.DomainModels.MassTransitRelay.Inbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Middleware;

public class ConsumeMessageOnceFilter<TConsumer, TMessage> : IFilter<ConsumerConsumeContext<TConsumer, TMessage>>
    where TConsumer : class
    where TMessage : class
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ConsumeMessageOnceFilter<TConsumer, TMessage>>();

    private readonly IConsumedMessagesContext consumedMessages;

    public ConsumeMessageOnceFilter(IConsumedMessagesContext consumedMessages)
    {
        this.consumedMessages = consumedMessages;
    }

    public void Probe(ProbeContext context) { }

    public async Task Send(
        ConsumerConsumeContext<TConsumer, TMessage> context,
        IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next
    )
    {
        var msg = ConsumedMessage.Create(context);

        if (
            await consumedMessages.ConsumedMessages
                .Where(m => m.ConsumerType == msg.ConsumerType && m.MessageId == msg.MessageId)
                .AnyAsync()
        )
        {
            logger.Information(
                "Message {MessageId} already consumed by {ConsumerType}",
                msg.MessageId,
                msg.ConsumerType
            );
            return;
        }
        else
        {
            consumedMessages.ConsumedMessages.Add(msg);
            await next.Send(context);
            await consumedMessages.SaveChangesAsync(context.CancellationToken);
        }
    }
}

public static class ConsumeMessageOnceFilterExtensions
{
    public static void UseConsumedMessagesFiltering(
        this IConsumePipeConfigurator configurator,
        IServiceProvider provider
    )
    {
        configurator.UseTypedConsumeFilter<Observer>(provider);
    }

    private sealed class Observer : ScopedTypedConsumerConsumePipeSpecificationObserver
    {
        public override void ConsumerMessageConfigured<TConsumer, TMessage>(
            IConsumerMessageConfigurator<TConsumer, TMessage> configurator
        ) =>
            configurator.AddConsumerScopedFilter<ConsumeMessageOnceFilter<TConsumer, TMessage>, TConsumer, TMessage>(
                Provider
            );
    }
}
