using Serilog;

namespace LeanCode.AuditLogs;

public class StubAuditLogStorage : IAuditLogStorage
{
    private readonly ILogger logger = Log.ForContext<StubAuditLogStorage>();

    public Task StoreEventAsync(AuditLogMessage auditLogMessage, CancellationToken cancellationToken)
    {
        logger.Information(
            "StubAuditLog: Changes found {UserId} {ActionName} {Type} {State} {@PrimaryKey} {@EntryChanged} {DateOccurred}",
            auditLogMessage.ActorId,
            auditLogMessage.ActionName,
            auditLogMessage.EntityChanged.Type,
            auditLogMessage.EntityChanged.EntityState,
            auditLogMessage.EntityChanged.Ids.Select(id => id.ToString()).ToList(),
            auditLogMessage.EntityChanged.Changes,
            auditLogMessage.DateOccurred
        );

        return Task.CompletedTask;
    }
}
