using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using MassTransit;
using Polly;

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
            var retryOnFailure = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(20, a => TimeSpan.FromSeconds(Math.Pow(2, a) < 120 ? Math.Pow(2, a) : 120));

            // The cast is important. Otherwise event will be published
            // as IDomainEvent interface instead of concrete object and handlers
            // won't be called.
            return retryOnFailure.ExecuteAsync(() => bus.Publish((object)evt, ctx =>
            {
                ctx.MessageId = evt.Id;
                ctx.ConversationId = correlationId;
            }));
        }
    }
}
