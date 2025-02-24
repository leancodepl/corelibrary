using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class NonProductionResponseLoggerMiddleware
{
    private readonly Serilog.ILogger logger;
    private readonly IHostEnvironment environment;

    public NonProductionResponseLoggerMiddleware(Serilog.ILogger logger, IHostEnvironment env)
    {
        this.logger = logger.ForContext<NonProductionResponseLoggerMiddleware>();
        environment = env;
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
