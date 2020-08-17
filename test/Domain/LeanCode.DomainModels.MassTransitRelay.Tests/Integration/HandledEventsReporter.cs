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

        public void ReportEvent(IConsumer consumer, ConsumeContext<TEvent> ctx) =>
            events.Enqueue(new HandledEvent(ctx.Message, consumer.GetType()));
    }

    public class HandledEvent
    {
        public IDomainEvent Event { get; }
        public Type ConsumerType { get; }

        public HandledEvent(IDomainEvent @event, Type consumerType)
        {
            Event = @event;
            ConsumerType = consumerType;
        }
    }
}
