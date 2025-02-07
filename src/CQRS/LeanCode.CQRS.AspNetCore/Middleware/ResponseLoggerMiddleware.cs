using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
        logger.LogInformation("Request executed with response {@Response}", result);
    }
}
