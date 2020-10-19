using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.Components.Startup;
using Microsoft.AspNetCore.Hosting;

namespace LeanCode.IntegrationTests.App
{
    public static class Program
    {
        [SuppressMessage("?", "IDE0060", Justification = "`args` are required by convention.")]
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return LeanProgram
                .BuildMinimalWebHost<Startup>()
                .UseKestrel()
                .AddAppConfigurationFromAzureKeyVaultOnNonDevelopmentEnvironment()
                .ConfigureDefaultLogging(
                    projectName: "integration-tests",
                    destructurers: new TypesCatalog(typeof(Program)));
        }
    }
}
