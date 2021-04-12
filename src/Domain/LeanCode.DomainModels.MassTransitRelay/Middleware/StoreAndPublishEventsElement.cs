using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Middleware
{
    public class StoreAndPublishEventsElement<TContext, TInput, TOutput> : IPipelineElement<TContext, TInput, TOutput>
        where TContext : notnull, IPipelineContext
    {
        private readonly IEventPublisher publisher;
        private readonly EventsStore impl;
        private readonly AsyncEventsInterceptor interceptor;

        public StoreAndPublishEventsElement(
            IEventPublisher publisher,
            EventsStore impl,
            AsyncEventsInterceptor interceptor)
        {
            this.impl = impl;
            this.interceptor = interceptor;
            this.publisher = publisher;
        }

        public async Task<TOutput> ExecuteAsync(TContext ctx, TInput input, Func<TContext, TInput, Task<TOutput>> next)
        {
            var (result, events) = await interceptor.CaptureEventsOfAsync(() => next(ctx, input));

            await impl.StoreAndPublishEventsAsync(events, Guid.NewGuid(), publisher, ctx.CancellationToken);

            return result;
        }
    }

    public static class StoreAndPublishPipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, TInput, TOutput> StoreAndPublishEvents<TContext, TInput, TOutput>(
            this PipelineBuilder<TContext, TInput, TOutput> builder)
            where TContext : notnull, IPipelineContext
        {
            return builder.Use<StoreAndPublishEventsElement<TContext, TInput, TOutput>>();
        }
    }
}
