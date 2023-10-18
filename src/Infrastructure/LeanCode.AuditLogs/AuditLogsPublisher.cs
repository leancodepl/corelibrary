using System.Diagnostics;
using LeanCode.OpenTelemetry;
using LeanCode.TimeProvider;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.AuditLogs;

public class AuditLogsPublisher
{
    public async Task ExtractAndPublishAsync(
        DbContext dbContext,
        IBus bus,
        string actionName,
        CancellationToken cancellationToken
    )
    {
        var entitiesChanged = ChangedEntitiesExtractor.Extract(dbContext);
        if (entitiesChanged.Count != 0)
        {
            var actorId = Activity.Current?.GetBaggageItem(IdentityTraceBaggageHelpers.CurrentUserIdKey);
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
                            cancellationToken
                        )
                )
            );
        }
    }
}
