using Leancode.CQRS.MassTransitRelay;
using LeanCode.DomainModels.Model;
using MassTransit;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.MassTransitRelay.Middleware;

public class EventsPublisherMiddleware
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<EventsPublisherMiddleware>();
    private readonly RequestDelegate next;
    private readonly AsyncEventsInterceptor interceptor;
    private readonly EventsPublisherOptions options;

    public EventsPublisherMiddleware(
        RequestDelegate next,
        AsyncEventsInterceptor interceptor,
        EventsPublisherOptions? options = null
    )
    {
        this.next = next;
        this.interceptor = interceptor;
        this.options = options ?? EventsPublisherOptions.Default;
    }

    public async Task InvokeAsync(HttpContext httpContext, IPublishEndpoint publishEndpoint)
    {
        var events = await interceptor.CaptureEventsOfAsync(() => next(httpContext));

        if (events.Count > 0)
        {
            await PublishEventsAsync(publishEndpoint, events, httpContext, httpContext.RequestAborted);
        }
    }

    private Task PublishEventsAsync(
        IPublishEndpoint publishEndpoint,
        List<IDomainEvent> events,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        logger.Debug("Publishing {Count} raised events", events.Count);
        var conversationId = Guid.NewGuid();

        var publishTasks = events.Select(evt => PublishEventAsync(publishEndpoint, evt, httpContext, conversationId, ct));

        return Task.WhenAll(publishTasks);
    }

    private Task PublishEventAsync(
        IPublishEndpoint publishEndpoint,
        IDomainEvent evt,
        HttpContext httpContext,
        Guid conversationId,
        CancellationToken ct
    )
    {
        logger.Debug("Publishing event of type {DomainEvent}", evt.GetType());

        var actorId = httpContext.User.Claims.FirstOrDefault(c => c.Type == options.NameClaimType)?.Value;

        return publishEndpoint.Publish(
            (object)evt, // Cast is necessary to publish the event as it's type, not an `IDomainEvent`
            publishCtx =>
            {
                publishCtx.MessageId = evt.Id;
                publishCtx.ConversationId = conversationId;
                publishCtx.Headers.Set(ConsumerConfigurationExtensions.ActorIdentifier, actorId);
            },
            ct
        );
    }
}
