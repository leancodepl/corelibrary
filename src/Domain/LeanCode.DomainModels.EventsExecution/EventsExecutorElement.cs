using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;
using Polly;

namespace LeanCode.DomainModels.EventsExecution
{
    public class EventsExecutorElement<TContext, TInput, TOutput>
        : IPipelineElement<TContext, TInput, TOutput>
        where TContext : IEventsContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<EventsExecutorElement<TContext, TInput, TOutput>>();

        private readonly RetryPolicies policies;
        private readonly AsyncEventsInterceptor interceptor;
        private readonly IDomainEventHandlerResolver resolver;

        public EventsExecutorElement(
            RetryPolicies policies,
            AsyncEventsInterceptor interceptor,
            IDomainEventHandlerResolver resolver)
        {
            this.policies = policies;
            this.interceptor = interceptor;
            this.resolver = resolver;
        }

        public async Task<TOutput> ExecuteAsync(
            TContext ctx,
            TInput input,
            Func<TContext, TInput, Task<TOutput>> next)
        {
            var result = await next(ctx, input).ConfigureAwait(false);
            if (!(ctx.SavedEvents is null) && ctx.SavedEvents.Count > 0)
            {
                ctx.ExecutedHandlers = new List<(IDomainEvent, Type)>();
                ctx.FailedHandlers = new List<(IDomainEvent, Type)>();
                await ExecuteEvents(ctx);
            }

            return result;
        }

        private async Task ExecuteEvents(TContext ctx)
        {
            for (int i = 0; i < ctx.SavedEvents.Count; i++)
            {
                var domainEvent = ctx.SavedEvents[i];
                var eventType = domainEvent.GetType();
                var handlers = resolver.FindEventHandlers(eventType);
                logger.Debug(
                    "Executing event {Event} with {N} handlers",
                    domainEvent, handlers.Count);

                foreach (var h in handlers)
                {
                    logger.Verbose(
                        "Executing handler {Handler} with event {Event}",
                        h.UnderlyingHandler, domainEvent);
                    await ExecuteHandler(ctx, domainEvent, h);
                }
            }
        }

        private async Task ExecuteHandler(
            TContext ctx,
            IDomainEvent domainEvent,
            IDomainEventHandlerWrapper handler)
        {
            var result = await policies.EventHandlerPolicy
                .ExecuteAndCaptureAsync(async () =>
                {
                    interceptor.Prepare();
                    await handler.HandleAsync(domainEvent).ConfigureAwait(false);
                    return interceptor.CaptureQueue();
                })
                .ConfigureAwait(false);

            if (result.Outcome == OutcomeType.Failure)
            {
                ctx.FailedHandlers.Add((domainEvent, handler.UnderlyingHandler));

                logger.Fatal(
                    result.FinalException,
                    "Cannot execute handler {Handler} with event {Event}",
                    handler.UnderlyingHandler, domainEvent);
            }
            else
            {
                if (!result.Result.IsEmpty)
                {
                    ctx.SavedEvents.AddRange(result.Result);
                }

                ctx.ExecutedHandlers.Add((domainEvent, handler.UnderlyingHandler));
                logger.Information(
                    "Handler {Handler} with event {Event} executed successfully",
                    handler.UnderlyingHandler, domainEvent);
            }
        }
    }

    public static partial class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, TInput, TOutput> ExecuteEvents<TContext, TInput, TOutput>(
            this PipelineBuilder<TContext, TInput, TOutput> builder)
            where TContext : IEventsContext
        {
            return builder.Use<EventsExecutorElement<TContext, TInput, TOutput>>();
        }
    }
}
