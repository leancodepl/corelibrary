using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;
using Serilog.Context;

namespace LeanCode.Correlation
{
    public class CorrelationElement<TContext, TInput, TOutput>
        : IPipelineElement<TContext, TInput, TOutput>
        where TContext : ICorrelationContext
    {
        public async Task<TOutput> ExecuteAsync(
            TContext ctx,
            TInput input,
            Func<TContext, TInput, Task<TOutput>> next)
        {
            if (ctx.CorrelationId == default)
            {
                ctx.CorrelationId = Guid.NewGuid();
            }
            if (ctx.ExecutionId == default)
            {
                ctx.ExecutionId = Guid.NewGuid();
            }

            using (Correlate.Logs(ctx.CorrelationId))
            using (Correlate.ExecutionLogs(ctx.ExecutionId))
            {
                return await next(ctx, input);
            }
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, TInput, TOutput> Correlate<TContext, TInput, TOutput>(
            this PipelineBuilder<TContext, TInput, TOutput> builder)
            where TContext : ICorrelationContext
        {
            return builder.Use<CorrelationElement<TContext, TInput, TOutput>>();
        }
    }
}
