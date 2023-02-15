using LeanCode.Pipelines;
using Serilog;

namespace LeanCode.CQRS.Execution
{
    public class ResponseLoggerElement<TContext, TObj, TRes> : IPipelineElement<TContext, TObj, TRes>
        where TContext : notnull, IPipelineContext
    {
        private readonly ILogger logger;

        public ResponseLoggerElement()
        {
            logger = Log.ForContext<ResponseLoggerElement<TContext, TObj, TRes>>();
        }

        public ResponseLoggerElement(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<TRes> ExecuteAsync(
            TContext ctx,
            TObj input,
            Func<TContext, TObj, Task<TRes>> next)
        {
            var result = await next(ctx, input);
            logger.Information("Request executed with response {@Response}", result);
            return result;
        }
    }

    public static class LogResponsesnPipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, TObj, TRes> LogRespones<TContext, TObj, TRes>(
            this PipelineBuilder<TContext, TObj, TRes> builder)
            where TContext : notnull, IPipelineContext
        {
            return builder.Use<ResponseLoggerElement<TContext, TObj, TRes>>();
        }
    }
}
