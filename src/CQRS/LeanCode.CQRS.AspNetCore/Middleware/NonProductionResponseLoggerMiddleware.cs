using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class NonProductionResponseLoggerMiddleware
{
    private readonly ILogger<NonProductionResponseLoggerMiddleware> logger;
    private readonly IHostEnvironment environment;

    public NonProductionResponseLoggerMiddleware(
        ILogger<NonProductionResponseLoggerMiddleware> logger,
        IHostEnvironment env
    )
    {
        this.logger = logger;
        environment = env;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        await next(httpContext);

        var payload = httpContext.GetCQRSRequestPayload();

        if (!environment.IsProduction())
        {
            logger.LogInformation("Request executed with response {@Response}", payload.Result);
        }
    }
}
