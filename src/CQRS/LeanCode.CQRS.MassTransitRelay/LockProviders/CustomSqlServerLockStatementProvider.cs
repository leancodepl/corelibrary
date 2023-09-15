using MassTransit.EntityFrameworkCoreIntegration;

namespace LeanCode.CQRS.MassTransitRelay.LockProviders;

public class CustomSqlServerLockStatementProvider : CustomSqlLockStatementProvider
{
    public CustomSqlServerLockStatementProvider(bool enableSchemaCaching = true)
        : base(new SqlServerLockStatementFormatter(), enableSchemaCaching) { }

    public CustomSqlServerLockStatementProvider(string schemaName, bool enableSchemaCaching = true)
        : base(schemaName, new SqlServerLockStatementFormatter(), enableSchemaCaching) { }
}
