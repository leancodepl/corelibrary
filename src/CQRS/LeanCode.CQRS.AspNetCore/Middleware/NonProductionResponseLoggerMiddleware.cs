using LeanCode.CQRS.Execution;
using LeanCode.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class NonProductionResponseLoggerMiddleware
{
    private readonly IHostEnvironment hostEnvironment;
    private readonly ILogger<NonProductionResponseLoggerMiddleware> logger;

    public NonProductionResponseLoggerMiddleware(
        IHostEnvironment hostEnvironment,
        ILogger<NonProductionResponseLoggerMiddleware> logger
    )
    {
        this.hostEnvironment = hostEnvironment;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        await next(httpContext);

        var result = httpContext.GetCQRSExecutionResult();

        if (!hostEnvironment.IsProduction())
        {
            logger.Information("Request executed with response {@Response}", result);
        }
    }
}
