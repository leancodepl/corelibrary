using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSResponseSerializerMiddleware
{
    private readonly RequestDelegate next;

    public CQRSResponseSerializerMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await next(httpContext);
        if (!httpContext.IsHttpResponseSet())
        {
            await httpContext.SerializeCQRSResultAsync();
        }
    }
}
