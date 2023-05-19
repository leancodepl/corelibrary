using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class NonProductionResponseLoggerMiddleware
{
    private readonly ILogger logger;
    private readonly IHostEnvironment environment;

    public NonProductionResponseLoggerMiddleware(IHostEnvironment env)
    {
        environment = env;
        logger = Log.ForContext<NonProductionResponseLoggerMiddleware>();
    }

    public NonProductionResponseLoggerMiddleware(IHostEnvironment env, ILogger logger)
    {
        environment = env;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        await next(httpContext);

        var payload = httpContext.GetCQRSRequestPayload();

        if (!environment.IsProduction())
        {
            logger.Information("Request executed with response {@Response}", payload.Result);
        }
    }
}
