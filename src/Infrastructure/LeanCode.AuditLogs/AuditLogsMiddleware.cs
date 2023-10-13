using System.Diagnostics;
using LeanCode.OpenTelemetry;
using LeanCode.TimeProvider;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.AuditLogs;

public class AuditLogsMiddleware<TDbContext>
    where TDbContext : DbContext
{
    private readonly RequestDelegate next;

    public AuditLogsMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, TDbContext dbContext, IBus bus)
    {
        await next(httpContext);
        await AuditLogsPublisher.ExtractAndPublishAsync(
            dbContext,
            bus,
            httpContext.Request.Path.ToString()!,
            httpContext.RequestAborted
        );
    }
}
