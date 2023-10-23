using MassTransit;

namespace LeanCode.AuditLogs;

public class AuditLogsConsumer : IConsumer<AuditLogMessage>
{
    private readonly IAuditLogStorage auditLogStorage;

    public AuditLogsConsumer(IAuditLogStorage auditLogStorage)
    {
        this.auditLogStorage = auditLogStorage;
    }

    public Task Consume(ConsumeContext<AuditLogMessage> context)
    {
        return auditLogStorage.StoreEventAsync(context.Message, context.CancellationToken);
    }
}
