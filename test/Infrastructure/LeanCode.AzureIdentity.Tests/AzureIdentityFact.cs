using LeanCode.Test.Helpers;

namespace LeanCode.AzureIdentity.Tests;

public sealed class AzureIdentityFact : ExternalServiceFactAttribute
{
    public AzureIdentityFact(params string[] requiredEnvVariables)
    {
        RequiredEnvVariables = requiredEnvVariables.ToHashSet();
    }

    protected override IReadOnlyCollection<string> RequiredEnvVariables { get; }
}
