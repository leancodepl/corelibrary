using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class ResponseLoggerMiddleware
{
    private readonly ILogger logger;
    private readonly RequestDelegate next;

    public ResponseLoggerMiddleware(RequestDelegate next)
    {
        logger = Log.ForContext<ResponseLoggerMiddleware>();

        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await next(httpContext);
        var result = httpContext.GetCQRSRequestPayload().Result;
        logger.Information("Request executed with response {@Response}", result);
    }
}
