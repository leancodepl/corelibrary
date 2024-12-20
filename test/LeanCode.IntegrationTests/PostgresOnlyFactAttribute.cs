using LeanCode.Test.Helpers;

namespace LeanCode.IntegrationTests;

public sealed class PostgresOnlyFactAttribute : IntegrationFactAttribute
{
    public override IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
        base.GetTraits().Append(new("database", "postgres")).ToList();
}
