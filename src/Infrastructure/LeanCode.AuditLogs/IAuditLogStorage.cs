namespace LeanCode.AuditLogs;

public interface IAuditLogStorage
{
    public Task StoreEventAsync(AuditLogMessage auditLogMessage, CancellationToken cancellationToken);
}

public record AuditLogMessage(
    EntityData EntityChanged,
    string ActionName,
    DateTimeOffset DateOccurred,
    string? ActorId,
    string? TraceId,
    string? SpanId
);
