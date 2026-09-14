using System.Runtime.CompilerServices;
using LeanCode.Test.Helpers;

namespace LeanCode.IntegrationTests;

public sealed class PostgresOnlyFactAttribute : IntegrationFactAttribute
{
    public static bool IsPostgres => Environment.GetEnvironmentVariable(TestDatabaseConfig.ConfigEnvName) == "postgres";

    public PostgresOnlyFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = 0
    )
        : base(sourceFilePath, sourceLineNumber)
    {
        Skip = "Test requires PostgreSQL.";
        SkipType = typeof(PostgresOnlyFactAttribute);
        SkipUnless = nameof(IsPostgres);
    }

    public override IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
        base.GetTraits().Append(new("database", "postgres")).ToList();
}
