using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public interface IEventPublisher
    {
        Task PublishAsync(IDomainEvent evt, Guid? correlationId);
    }

    public class BusEventPublisher : IEventPublisher
    {
        private readonly IBus bus;

        public BusEventPublisher(IBus bus)
        {
            this.bus = bus;
        }

        public Task PublishAsync(IDomainEvent evt, Guid? correlationId)
        {
            // The cast is important. Otherwise event will be published
            // as IDomainEvent interface instead of concrete object and handlers
            // won't be called.
            return bus.Publish((object)evt, ctx =>
            {
                ctx.MessageId = evt.Id;
                ctx.ConversationId = correlationId;
            });
        }
    }
}
