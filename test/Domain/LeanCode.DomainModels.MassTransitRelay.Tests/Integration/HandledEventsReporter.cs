using System;
using System.Collections.Concurrent;
using LeanCode.DomainModels.Model;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    public class HandledEventsReporter<TEvent>
        where TEvent : class, IDomainEvent
    {
        private readonly ConcurrentQueue<HandledEvent> events = new ConcurrentQueue<HandledEvent>();
        public HandledEvent[] HandledEvents => events.ToArray();

        public void ReportEvent(IConsumer consumer, ConsumeContext<TEvent> ctx)
        {
            events.Enqueue(new HandledEvent(ctx.Message, ctx.ConversationId ?? Guid.Empty, consumer.GetType()));
        }
    }

    public struct HandledEvent
    {
        public IDomainEvent Event { get; }
        public Guid CorrelationId { get; }
        public Type ConsumerType { get; }

        public HandledEvent(IDomainEvent @event, Guid correlationId, Type consumerType)
        {
            CorrelationId = correlationId;
            Event = @event;
            ConsumerType = consumerType;
        }
    }
}
