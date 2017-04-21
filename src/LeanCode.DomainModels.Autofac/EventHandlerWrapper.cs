using System.Threading.Tasks;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.Autofac
{
    public class EventHandlerWrapper<TEvent> : IDomainEventHandlerWrapper
        where TEvent : IDomainEvent
    {
        private readonly IDomainEventHandler<TEvent> handler;

        public EventHandlerWrapper(IDomainEventHandler<TEvent> handler)
        {
            this.handler = handler;
        }

        public Task HandleAsync(IDomainEvent domainEvent)
        {
            return handler.HandleAsync((TEvent)domainEvent);
        }
    }
}
