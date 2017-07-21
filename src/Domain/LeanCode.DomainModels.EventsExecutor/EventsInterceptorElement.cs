using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecutor
{
    public class EventsInterceptorElement<TContext, TInput, TOutput>
        : IPipelineElement<TContext, TInput, TOutput>
        where TContext : IEventsContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<EventsInterceptorElement<TContext, TInput, TOutput>>();

        private readonly AsyncEventsInterceptor interceptor;

        public EventsInterceptorElement(AsyncEventsInterceptor interceptor)
        {
            this.interceptor = interceptor;
        }

        public async Task<TOutput> ExecuteAsync(
            TContext ctx,
            TInput input,
            Func<TContext, TInput, Task<TOutput>> next)
        {
            logger.Debug("Preparing async events interceptor for the request");
            interceptor.Prepare();
            try
            {
                var result = await next(ctx, input).ConfigureAwait(false);

                var queue = interceptor.CaptureQueue();
                logger.Debug("{EventCount} events captured, saving", queue.Count);
                ctx.SavedEvents = new List<IDomainEvent>(queue);

                return result;
            }
            catch
            {
                logger.Debug("Cannot execute the rest of the pipeline, skipping events capture");
                throw;
            }
        }
    }

    public static partial class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, TInput, TOutput> InterceptEvents<TContext, TInput, TOutput>(
            this PipelineBuilder<TContext, TInput, TOutput> builder)
            where TContext : IEventsContext
        {
            return builder.Use<EventsInterceptorElement<TContext, TInput, TOutput>>();
        }
    }
}
