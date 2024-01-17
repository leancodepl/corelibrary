using MassTransit.EntityFrameworkCoreIntegration;

namespace LeanCode.CQRS.MassTransitRelay.LockProviders;

public class CustomSqlServerLockStatementProvider : CustomSqlLockStatementProvider
{
    public CustomSqlServerLockStatementProvider(bool enableSchemaCaching = true, bool serializable = false)
        : base(new SqlServerLockStatementFormatter(serializable), enableSchemaCaching) { }

    public CustomSqlServerLockStatementProvider(
        string schemaName,
        bool enableSchemaCaching = true,
        bool serializable = false
    )
        : base(schemaName, new SqlServerLockStatementFormatter(serializable), enableSchemaCaching) { }
}
