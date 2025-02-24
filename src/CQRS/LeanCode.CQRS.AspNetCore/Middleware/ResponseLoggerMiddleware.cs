using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class ResponseLoggerMiddleware
{
    private readonly Serilog.ILogger logger;
    private readonly RequestDelegate next;

    public ResponseLoggerMiddleware(Serilog.ILogger logger, RequestDelegate next)
    {
        this.logger = logger.ForContext<ResponseLoggerMiddleware>();
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await next(httpContext);
        var result = httpContext.GetCQRSRequestPayload().Result;
        logger.Information("Request executed with response {@Response}", result);
    }
}
