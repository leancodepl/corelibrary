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

    public async Task InvokeAsync(
        HttpContext httpContext,
        TDbContext dbContext,
        IBus bus,
        AuditLogsPublisher auditLogsPublisher
    )
    {
        await next(httpContext);
        await auditLogsPublisher.ExtractAndPublishAsync(
            dbContext,
            bus,
            httpContext.Request.Path.ToString()!,
            httpContext.RequestAborted
        );
    }
}
