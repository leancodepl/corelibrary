using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LeanCode.Pipelines;
using OpenTelemetry.Trace;

namespace LeanCode.OpenTelemetry
{
    public class TracingElement<TContext, TInput, TOutput> : IPipelineElement<TContext, TInput, TOutput>
        where TContext : IPipelineContext
    {
        public async Task<TOutput> ExecuteAsync(TContext ctx, TInput input, Func<TContext, TInput, Task<TOutput>> next)
        {
            using var activity = LeanCodeActivitySource.ActivitySource.StartActivity("pipeline.action");
            activity?.AddTag("object", typeof(TInput).FullName);

            try
            {
                return await next(ctx, input);
            }
            catch
            {
                activity?.SetStatus(Status.Internal);
                throw;
            }
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, TInput, TOutput> Trace<TContext, TInput, TOutput>(
            this PipelineBuilder<TContext, TInput, TOutput> builder)
            where TContext : IPipelineContext
        {
            return builder.Use<TracingElement<TContext, TInput, TOutput>>();
        }
    }
}
