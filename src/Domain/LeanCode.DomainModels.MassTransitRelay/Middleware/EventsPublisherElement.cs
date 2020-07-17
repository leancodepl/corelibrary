using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.Correlation;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Middleware
{
    public class EventsPublisherElement<TContext, TInput, TOutput> : IPipelineElement<TContext, TInput, TOutput>
        where TContext : notnull, ICorrelationContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<EventsPublisherElement<TContext, TInput, TOutput>>();
        private readonly IEventPublisher publisher;
        private readonly AsyncEventsInterceptor interceptor;

        public EventsPublisherElement(IEventPublisher publisher, AsyncEventsInterceptor interceptor)
        {
            this.interceptor = interceptor;
            this.publisher = publisher;
        }

        public async Task<TOutput> ExecuteAsync(TContext ctx, TInput input, Func<TContext, TInput, Task<TOutput>> next)
        {
            var (result, events) = await interceptor.CaptureEventsOf(() => next(ctx, input));

            if (events.Count > 0)
            {
                await PublishEventsAsync(ctx, events);
            }

            return result;
        }

        private Task PublishEventsAsync(TContext ctx, List<IDomainEvent> events)
        {
            logger.Debug("Publishing {Count} raised events", events.Count);

            var publishTasks = events
                .Select(evt => PublishEventAsync(evt, ctx.CorrelationId));

            return Task.WhenAll(publishTasks);
        }

        private async Task PublishEventAsync(IDomainEvent evt, Guid correlationId)
        {
            logger.Debug("Publishing event of type {DomainEvent}", evt);

            try
            {
                await publisher.PublishAsync(evt, correlationId);
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Could not publish event {@DomainEvent}", evt);
            }

            logger.Information("Domain event {DomainEvent} published", evt);
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, TInput, TOutput> PublishEvents<TContext, TInput, TOutput>(
            this PipelineBuilder<TContext, TInput, TOutput> builder)
            where TContext : notnull, ICorrelationContext
        {
            return builder.Use<EventsPublisherElement<TContext, TInput, TOutput>>();
        }
    }
}
