using System.Diagnostics;
using System.Reflection;
using LeanCode.IntegrationTestHelpers;
using LeanCode.IntegrationTests.App;
using LeanCode.Logging;
using LeanCode.Startup.MicrosoftDI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace LeanCode.IntegrationTests;

public class TestApp : LeanCodeTestFactory<App.Startup>
{
    static TestApp()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAIT_FOR_DEBUGGER")))
        {
            Console.WriteLine("Waiting for debugger to be attached...");

            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }

            Console.WriteLine("Debugger attached");
        }
    }

    protected override ConfigurationOverrides Configuration => TestDatabaseConfig.Create().GetConfigurationOverrides();

    protected override IEnumerable<Assembly> GetTestAssemblies()
    {
        yield return typeof(App.Startup).Assembly;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSolutionRelativeContentRoot("test/LeanCode.IntegrationTests");
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return LeanProgram
            .BuildMinimalHost<App.Startup>()
            .ConfigureDefaultLogging(projectName: "test", destructurers: new[] { typeof(Program).Assembly })
            .UseEnvironment(Environments.Development);
    }
}
