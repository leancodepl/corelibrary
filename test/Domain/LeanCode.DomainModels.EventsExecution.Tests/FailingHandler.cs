using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.EventsExecution.Tests
{
    public class FailingHandler<TEvent> : IDomainEventHandler<TEvent>
        where TEvent : IDomainEvent
    {
        private readonly IDomainEvent[] events;

        public FailingHandler(params IDomainEvent[] events)
        {
            this.events = events;
        }

        public Task HandleAsync(TEvent domainEvent)
        {
            foreach (var e in events)
            {
                DomainEvents.Raise(e);
            }

            throw new Exception("Failed!");
        }
    }
}
