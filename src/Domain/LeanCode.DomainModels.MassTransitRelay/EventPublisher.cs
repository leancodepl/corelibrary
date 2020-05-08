using System;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public interface IEventPublisher
    {
        Task PublishAsync(object evt, Guid eventId, Guid? correlationId, CancellationToken cancellationToken = default);
        Task PublishAsync(IDomainEvent evt, Guid? correlationId, CancellationToken cancellationToken = default)
            => PublishAsync(evt, evt.Id, correlationId, cancellationToken);
    }

    public class EventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint publishEndpoint;

        public EventPublisher(IPublishEndpoint publishEndpoint)
        {
            this.publishEndpoint = publishEndpoint;
        }

        public Task PublishAsync(object evt, Guid eventId, Guid? correlationId, CancellationToken cancellationToken = default)
        {
            return publishEndpoint.Publish(evt, ctx =>
            {
                ctx.MessageId = eventId;
                ctx.ConversationId = correlationId;
            }, cancellationToken);
        }
    }
}
