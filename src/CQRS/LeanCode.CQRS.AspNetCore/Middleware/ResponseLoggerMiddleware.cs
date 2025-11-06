using LeanCode.CQRS.Execution;
using LeanCode.Logging;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class ResponseLoggerMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ResponseLoggerMiddleware> logger;

    public ResponseLoggerMiddleware(RequestDelegate next, ILogger<ResponseLoggerMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await next(httpContext);
        var result = httpContext.GetCQRSExecutionResult();
        logger.Information("Request executed with response {@Response}", result);
    }
}
