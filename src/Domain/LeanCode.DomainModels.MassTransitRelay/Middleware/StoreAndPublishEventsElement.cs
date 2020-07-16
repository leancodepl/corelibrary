using System;
using System.Threading.Tasks;
using LeanCode.Correlation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Middleware
{
    public class StoreAndPublishEventsElement<TContext, TInput, TOutput> : IPipelineElement<TContext, TInput, TOutput>
        where TContext : notnull, ICorrelationContext, IEventsInterceptorContext
    {
        private readonly IEventPublisher publisher;
        private readonly EventsStore impl;

        public StoreAndPublishEventsElement(
            IEventPublisher publisher,
            EventsStore impl)
        {
            this.impl = impl;
            this.publisher = publisher;
        }

        public async Task<TOutput> ExecuteAsync(TContext ctx, TInput input, Func<TContext, TInput, Task<TOutput>> next)
        {
            var result = await next(ctx, input);

            await impl.StoreAndPublishEventsAsync(ctx.SavedEvents, ctx.CorrelationId, publisher);

            return result;
        }
    }

    public static class StoreAndPublishPipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, TInput, TOutput> StoreAndPublishEvents<TContext, TInput, TOutput>(
            this PipelineBuilder<TContext, TInput, TOutput> builder)
            where TContext : notnull, IEventsInterceptorContext, ICorrelationContext
        {
            return builder.Use<StoreAndPublishEventsElement<TContext, TInput, TOutput>>();
        }
    }
}
