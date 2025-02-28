using LeanCode.CQRS.Execution;
using LeanCode.Logging;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class ResponseLoggerMiddleware
{
    private readonly ILogger<ResponseLoggerMiddleware> logger;
    private readonly RequestDelegate next;

    public ResponseLoggerMiddleware(ILogger<ResponseLoggerMiddleware> logger, RequestDelegate next)
    {
        this.logger = logger;
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await next(httpContext);
        var result = httpContext.GetCQRSRequestPayload().Result;
        logger.Information("Request executed with response {@Response}", result);
    }
}
