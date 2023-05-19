using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class ResponseLoggerMiddleware
{
    private readonly ILogger logger;

    public ResponseLoggerMiddleware()
    {
        logger = Log.ForContext<ResponseLoggerMiddleware>();
    }

    public ResponseLoggerMiddleware(ILogger logger)
    {
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        await next(httpContext);
        var result = httpContext.GetCQRSRequestPayload().Result;
        logger.Information("Request executed with response {@Response}", result);
    }
}
