using System.Diagnostics;
using System.Text.Json;
using LeanCode.OpenTelemetry;
using LeanCode.TimeProvider;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.AuditLogs;

public class AuditLogsPublisher
{
    private readonly JsonSerializerOptions? jsonSerializerOptions;

    public AuditLogsPublisher()
        : this(null) { }

    public AuditLogsPublisher(
        [FromKeyedServices(AuditLogsExtensions.JsonSerializerOptionsKey)] JsonSerializerOptions? jsonSerializerOptions
    )
    {
        this.jsonSerializerOptions = jsonSerializerOptions;
    }

    public virtual async Task ExtractAndPublishAsync(
        DbContext dbContext,
        IPublishEndpoint bus,
        string actionName,
        CancellationToken cancellationToken
    )
    {
        var entitiesChanged = ChangedEntitiesExtractor.Extract(dbContext, jsonSerializerOptions);
        if (entitiesChanged.Count != 0)
        {
            var actorId = Activity.Current?.GetBaggageItem(IdentityTraceBaggageHelpers.CurrentUserIdKey);
            var now = Time.NowWithOffset;

            await Task.WhenAll(
                entitiesChanged.Select(e =>
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
