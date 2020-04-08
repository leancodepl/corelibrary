using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using MassTransit;
using Polly;
using Polly.Retry;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public interface IEventPublisher
    {
        Task PublishAsync(IDomainEvent evt, Guid? correlationId);
    }

    public class BusEventPublisher : IEventPublisher
    {
        private static readonly AsyncRetryPolicy RetryOnFailure = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(20, a => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, a), 120)));

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
            return RetryOnFailure.ExecuteAsync(() => bus.Publish((object)evt, ctx =>
            {
                ctx.MessageId = evt.Id;
                ctx.ConversationId = correlationId;
            }));
        }
    }
}
