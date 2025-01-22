namespace LeanCode.AuditLogs;

public interface IAuditLogStorage
{
    Task StoreEventAsync(AuditLogMessage auditLogMessage, CancellationToken cancellationToken);
}

public record AuditLogMessage(
    EntityData EntityChanged,
    string ActionName,
    DateTimeOffset DateOccurred,
    string? ActorId,
    string? TraceId,
    string? SpanId
);
