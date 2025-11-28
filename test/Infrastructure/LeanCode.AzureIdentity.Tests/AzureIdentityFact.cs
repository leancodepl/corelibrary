using System.Runtime.CompilerServices;
using LeanCode.Test.Helpers;

namespace LeanCode.AzureIdentity.Tests;

public sealed class AzureIdentityFact : ExternalServiceFactAttribute
{
    protected override string ServiceType => "azure-identity";
    protected override IReadOnlyCollection<string> RequiredEnvVariables { get; }

    public AzureIdentityFact(
        string[] requiredEnvVariables,
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = 0
    )
        : base(sourceFilePath, sourceLineNumber)
    {
        RequiredEnvVariables = requiredEnvVariables.ToHashSet();
    }
}
