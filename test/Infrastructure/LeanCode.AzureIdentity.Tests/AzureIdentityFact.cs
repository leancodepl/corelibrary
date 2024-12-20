using LeanCode.Test.Helpers;

namespace LeanCode.AzureIdentity.Tests;

public sealed class AzureIdentityFact : ExternalServiceFactAttribute
{
    protected override string ServiceType => "azure-identity";
    protected override IReadOnlyCollection<string> RequiredEnvVariables { get; }

    public AzureIdentityFact(params string[] requiredEnvVariables)
    {
        RequiredEnvVariables = requiredEnvVariables.ToHashSet();
    }
}
