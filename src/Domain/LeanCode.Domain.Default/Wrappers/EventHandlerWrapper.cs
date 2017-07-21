using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.DomainModels.Model;

namespace LeanCode.Domain.Default.Wrappers
{
    public class EventHandlerWrapper<TEvent> : IDomainEventHandlerWrapper
        where TEvent : IDomainEvent
    {
        private readonly IDomainEventHandler<TEvent> handler;

        public Type UnderlyingHandler { get; }

        public EventHandlerWrapper(IDomainEventHandler<TEvent> handler)
        {
            this.handler = handler;
            UnderlyingHandler = handler.GetType();
        }

        public Task HandleAsync(IDomainEvent domainEvent)
        {
            return handler.HandleAsync((TEvent)domainEvent);
        }
    }
}
