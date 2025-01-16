using System.Reflection;
using LeanCode.IntegrationTestHelpers.Tests.App;
using LeanCode.Logging;
using LeanCode.Startup.MicrosoftDI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Events;

namespace LeanCode.IntegrationTestHelpers.Tests;

public class TestApp : LeanCodeTestFactory<App.Startup>
{
    protected override TestConnectionString ConnectionStringConfig { get; } =
        new("SqlServer:ConnectionStringBase", "SqlServer:ConnectionString");

    protected override ConfigurationOverrides ConfigurationOverrides { get; } =
        new(
            new()
            {
                [IHostBuilderExtensions.MinimumLogLevelKey] = "Error",
                [IHostBuilderExtensions.EnableDetailedInternalLogsKey] = "false",
                ["SqlServer:ConnectionStringBase"] = "",
            }
        );

    protected override IEnumerable<Assembly> GetTestAssemblies()
    {
        yield return typeof(App.Startup).Assembly;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Slight hack - not calling base to not register BusActivityMonitor
        // Seems easier for this special case then setting up MassTransit
        // base.ConfigureWebHost(builder);

        builder.UseSolutionRelativeContentRoot("test/Testing/LeanCode.IntegrationTestHelpers.Tests");

        builder
            .ConfigureAppConfiguration(config =>
            {
                config.Add(ConfigurationOverrides);
                config.Add(ConnectionStringConfig);
            })
            .ConfigureServices(services =>
            {
                services.AddAuthentication(TestAuthenticationHandler.SchemeName).AddTestAuthenticationHandler();

                // Allow the host to perform shutdown a little bit longer - it will make
                // `DbContextsInitializer` successfully drop the database more frequently. :)
                services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));
            });
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
