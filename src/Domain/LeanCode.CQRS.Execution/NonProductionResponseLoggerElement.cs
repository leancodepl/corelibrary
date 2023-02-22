using LeanCode.Pipelines;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LeanCode.CQRS.Execution;

public class NonProductionResponseLoggerElement<TContext, TObj, TRes> : IPipelineElement<TContext, TObj, TRes>
    where TContext : notnull, IPipelineContext
{
    private readonly ILogger logger;
    private readonly IHostEnvironment environment;

    public NonProductionResponseLoggerElement(IHostEnvironment env)
    {
        environment = env;
        logger = Log.ForContext<NonProductionResponseLoggerElement<TContext, TObj, TRes>>();
    }

    public NonProductionResponseLoggerElement(IHostEnvironment env, ILogger logger)
    {
        environment = env;
        this.logger = logger;
    }

    public async Task<TRes> ExecuteAsync(TContext ctx, TObj input, Func<TContext, TObj, Task<TRes>> next)
    {
        var result = await next(ctx, input);

        if (!environment.IsProduction())
        {
            logger.Information("Request executed with response {@Response}", result);
        }

        return result;
    }
}

public static class LogResponsesnOnNonProductionPipelineBuilderExtensions
{
    public static PipelineBuilder<TContext, TObj, TRes> LogResponesOnNonProduction<TContext, TObj, TRes>(
        this PipelineBuilder<TContext, TObj, TRes> builder
    )
        where TContext : notnull, IPipelineContext
    {
        return builder.Use<NonProductionResponseLoggerElement<TContext, TObj, TRes>>();
    }
}
