using System.Diagnostics.CodeAnalysis;
using LeanCode.Components;
using LeanCode.Components.Startup;
using Microsoft.Extensions.Hosting;

namespace LeanCode.IntegrationTests.App;

public static class Program
{
    [SuppressMessage("?", "IDE0060", Justification = "`args` are required by convention.")]
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return LeanProgram
            .BuildMinimalHost<Startup>()
            .AddAppConfigurationFromAzureKeyVaultOnNonDevelopmentEnvironment()
            .ConfigureDefaultLogging(projectName: "test", destructurers: new TypesCatalog(typeof(Program)));
    }
}
