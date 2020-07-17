﻿using System;
using System.Threading.Tasks;
using LeanCode.Correlation;
using LeanCode.DomainModels.EventsExecution;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Middleware
{
    public class StoreAndPublishEventsElement<TContext, TInput, TOutput> : IPipelineElement<TContext, TInput, TOutput>
        where TContext : notnull, ICorrelationContext
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
            var (result, events) = await interceptor.CaptureEventsOf(() => next(ctx, input));

            await impl.StoreAndPublishEventsAsync(events, ctx.CorrelationId, publisher);

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