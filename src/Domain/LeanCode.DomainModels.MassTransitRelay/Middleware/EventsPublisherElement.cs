using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;
using MassTransit;

namespace LeanCode.DomainModels.MassTransitRelay.Middleware;

public class EventsPublisherElement<TContext, TInput, TOutput> : IPipelineElement<TContext, TInput, TOutput>
    where TContext : notnull, IPipelineContext
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<
        EventsPublisherElement<TContext, TInput, TOutput>
    >();
    private readonly IPublishEndpoint publishEndpoint;
    private readonly AsyncEventsInterceptor interceptor;

    public EventsPublisherElement(IPublishEndpoint publishEndpoint, AsyncEventsInterceptor interceptor)
    {
        this.publishEndpoint = publishEndpoint;
        this.interceptor = interceptor;
    }

    public async Task<TOutput> ExecuteAsync(TContext ctx, TInput input, Func<TContext, TInput, Task<TOutput>> next)
    {
        var (result, events) = await interceptor.CaptureEventsOfAsync(() => next(ctx, input));

        if (events.Count > 0)
        {
            await PublishEventsAsync(ctx, events);
        }

        return result;
    }

    private Task PublishEventsAsync(TContext ctx, List<IDomainEvent> events)
    {
        logger.Debug("Publishing {Count} raised events", events.Count);
        var conversationId = Guid.NewGuid();

        var publishTasks = events.Select(evt => PublishEventAsync(evt, ctx, conversationId));

        return Task.WhenAll(publishTasks);
    }

    private Task PublishEventAsync(IDomainEvent evt, TContext ctx, Guid conversationId)
    {
        logger.Debug("Publishing event of type {DomainEvent}", evt);
        return publishEndpoint.Publish(
            (object)evt,
            publishCtx =>
            {
                publishCtx.MessageId = evt.Id;
                publishCtx.ConversationId = conversationId;
            },
            ctx.CancellationToken
        );
    }
}

public static class PipelineBuilderExtensions
{
    public static PipelineBuilder<TContext, TInput, TOutput> PublishEvents<TContext, TInput, TOutput>(
        this PipelineBuilder<TContext, TInput, TOutput> builder
    )
        where TContext : notnull, IPipelineContext
    {
        return builder.Use<EventsPublisherElement<TContext, TInput, TOutput>>();
    }
}
