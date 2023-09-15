using MassTransit.EntityFrameworkCoreIntegration;

namespace LeanCode.CQRS.MassTransitRelay.LockProviders;

public class CustomSqliteLockStatementProvider : CustomSqlLockStatementProvider
{
    public CustomSqliteLockStatementProvider(bool enableSchemaCaching = true)
        : base(new SqliteLockStatementFormatter(), enableSchemaCaching) { }

    public CustomSqliteLockStatementProvider(string schemaName, bool enableSchemaCaching = true)
        : base(schemaName, new SqliteLockStatementFormatter(), enableSchemaCaching) { }
}
