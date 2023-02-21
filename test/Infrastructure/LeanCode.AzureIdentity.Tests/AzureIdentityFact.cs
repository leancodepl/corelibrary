using Xunit;

namespace LeanCode.AzureIdentity.Tests;

public sealed class AzureIdentityFact : FactAttribute
{
    public AzureIdentityFact(params string[] requiredEnvVariables)
    {
        if (!VariablesSet(requiredEnvVariables))
        {
            Skip = "Azure Identity configuration not set";
        }
    }

    private static bool VariablesSet(string[] variables)
    {
        return variables.All(v => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(v)));
    }
}
