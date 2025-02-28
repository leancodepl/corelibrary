using LeanCode.DomainModels.Model;
using LeanCode.Logging;
using MassTransit;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.MassTransitRelay.Middleware;

public class EventsPublisherMiddleware
{
    private readonly ILogger<EventsPublisherMiddleware> logger;
    private readonly RequestDelegate next;
    private readonly AsyncEventsInterceptor interceptor;

    public EventsPublisherMiddleware(
        ILogger<EventsPublisherMiddleware> logger,
        RequestDelegate next,
        AsyncEventsInterceptor interceptor
    )
    {
        this.logger = logger;
        this.next = next;
        this.interceptor = interceptor;
    }

    public async Task InvokeAsync(HttpContext httpContext, IPublishEndpoint publishEndpoint)
    {
        var events = await interceptor.CaptureEventsOfAsync(() => next(httpContext));

        if (events.Count > 0)
        {
            await PublishEventsAsync(publishEndpoint, events, httpContext.RequestAborted);
        }
    }

    private Task PublishEventsAsync(
        IPublishEndpoint publishEndpoint,
        List<IDomainEvent> events,
        CancellationToken cancellationToken
    )
    {
        logger.Debug("Publishing {Count} raised events", events.Count);
        var conversationId = Guid.NewGuid();

        var publishTasks = events.Select(evt =>
            PublishEventAsync(publishEndpoint, evt, conversationId, cancellationToken)
        );

        return Task.WhenAll(publishTasks);
    }

    private Task PublishEventAsync(
        IPublishEndpoint publishEndpoint,
        IDomainEvent evt,
        Guid conversationId,
        CancellationToken cancellationToken
    )
    {
        logger.Debug("Publishing event of type {DomainEvent}", evt.GetType());
        return publishEndpoint.Publish(
            (object)evt, // Cast is necessary to publish the event as it's type, not an `IDomainEvent`
            publishCtx =>
            {
                publishCtx.MessageId = evt.Id;
                publishCtx.ConversationId = conversationId;
            },
            cancellationToken
        );
    }
}
