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

        var entitiesChanged = ChangedEntitiesExtractor.Extract(dbContext);
        if (entitiesChanged.Count != 0)
        {
            var actorId = Activity.Current?.GetBaggageItem(IdentityTraceBaggageHelpers.CurrentUserIdKey);
            var actionName = httpContext.Request.Path.ToString();
            var now = Time.NowWithOffset;

            await Task.WhenAll(
                entitiesChanged.Select(
                    e =>
                        bus.Publish(
                            new AuditLogMessage(
                                e,
                                actionName,
                                now,
                                actorId,
                                Activity.Current?.TraceId.ToString(),
                                Activity.Current?.SpanId.ToString()
                            ),
                            httpContext.RequestAborted
                        )
                )
            );
        }
    }
}
