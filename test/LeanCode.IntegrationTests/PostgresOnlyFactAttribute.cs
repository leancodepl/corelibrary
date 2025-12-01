using System.Runtime.CompilerServices;
using LeanCode.Test.Helpers;

namespace LeanCode.IntegrationTests;

public sealed class PostgresOnlyFactAttribute(
    [CallerFilePath] string? sourceFilePath = null,
    [CallerLineNumber] int sourceLineNumber = 0
) : IntegrationFactAttribute(sourceFilePath, sourceLineNumber)
{
    public override IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
        base.GetTraits().Append(new("database", "postgres")).ToList();
}
