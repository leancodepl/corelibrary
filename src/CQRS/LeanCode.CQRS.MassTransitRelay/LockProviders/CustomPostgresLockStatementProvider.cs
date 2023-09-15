using MassTransit.EntityFrameworkCoreIntegration;

namespace LeanCode.CQRS.MassTransitRelay.LockProviders;

public class CustomPostgresLockStatementProvider : CustomSqlLockStatementProvider
{
    public CustomPostgresLockStatementProvider(bool enableSchemaCaching = true)
        : base(new PostgresLockStatementFormatter(), enableSchemaCaching) { }

    public CustomPostgresLockStatementProvider(string schemaName, bool enableSchemaCaching = true)
        : base(schemaName, new PostgresLockStatementFormatter(), enableSchemaCaching) { }
}
