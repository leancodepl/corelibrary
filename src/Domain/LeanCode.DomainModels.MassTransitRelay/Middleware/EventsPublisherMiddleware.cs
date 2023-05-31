using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;
using MassTransit;
using Microsoft.AspNetCore.Http;

namespace LeanCode.DomainModels.MassTransitRelay.Middleware;

public class EventsPublisherMiddleware
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<EventsPublisherMiddleware>();
    private readonly RequestDelegate next;
    private readonly AsyncEventsInterceptor interceptor;

    public EventsPublisherMiddleware(RequestDelegate next, AsyncEventsInterceptor interceptor)
    {
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

    private Task PublishEventsAsync(IPublishEndpoint publishEndpoint, List<IDomainEvent> events, CancellationToken ct)
    {
        logger.Debug("Publishing {Count} raised events", events.Count);
        var conversationId = Guid.NewGuid();

        var publishTasks = events.Select(evt => PublishEventAsync(publishEndpoint, evt, conversationId, ct));

        return Task.WhenAll(publishTasks);
    }

    private Task PublishEventAsync(
        IPublishEndpoint publishEndpoint,
        IDomainEvent evt,
        Guid conversationId,
        CancellationToken ct
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
            ct
        );
    }
}
