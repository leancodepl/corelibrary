using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.Components.Startup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ExampleApp.Api;

public class Program
{
    public static Task Main() => CreateWebHostBuilder().Build().RunAsync();

    public static IHostBuilder CreateWebHostBuilder()
    {
        return LeanProgram
            .BuildMinimalHost<Startup>()
            .AddAppConfigurationFromAzureKeyVaultOnNonDevelopmentEnvironment()
            .ConfigureDefaultLogging("ExampleApp", TypesCatalog.Of<Startup>());
    }
}
