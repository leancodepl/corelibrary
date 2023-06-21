using System.Reflection;
using LeanCode.IntegrationTestHelpers.Tests.App;
using LeanCode.Logging;
using LeanCode.Startup.MicrosoftDI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace LeanCode.IntegrationTestHelpers.Tests;

public class TestApp : LeanCodeTestFactory<App.Startup>
{
    protected override ConfigurationOverrides Configuration { get; } =
        new ConfigurationOverrides(Serilog.Events.LogEventLevel.Error, false);

    protected override IEnumerable<Assembly> GetTestAssemblies()
    {
        yield return typeof(App.Startup).Assembly;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSolutionRelativeContentRoot("test/Testing/LeanCode.IntegrationTestHelpers.Tests");
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return LeanProgram
            .BuildMinimalHost<App.Startup>()
            .ConfigureDefaultLogging(
                projectName: "integration-tests",
                destructurers: new[] { typeof(Program).Assembly }
            )
            .UseEnvironment(Environments.Development);
    }
}
