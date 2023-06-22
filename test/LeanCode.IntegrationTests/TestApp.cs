using System.Diagnostics;
using System.Reflection;
using FluentAssertions;
using LeanCode.CQRS.MassTransitRelay;
using LeanCode.IntegrationTestHelpers;
using LeanCode.IntegrationTests.App;
using LeanCode.Logging;
using LeanCode.Startup.MicrosoftDI;
using MassTransit.Middleware.Outbox;
using MassTransit.Testing.Implementations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Events;

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

    protected override ConfigurationOverrides Configuration { get; } = new(LogEventLevel.Debug);

    public async Task WaitForBusAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await Services.GetRequiredService<IBusOutboxNotification>().WaitForDelivery(cts.Token);

        var result = await Services
            .GetRequiredService<IBusActivityMonitor>()
            .AwaitBusInactivity(TimeSpan.FromSeconds(5));

        result.Should().BeTrue("bus should process messages");
    }

    protected override IEnumerable<Assembly> GetTestAssemblies()
    {
        yield return typeof(App.Startup).Assembly;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.AddBusActivityMonitor();
        });
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return LeanProgram
            .BuildMinimalHost<App.Startup>()
            .ConfigureDefaultLogging(projectName: "test", destructurers: new[] { typeof(Program).Assembly })
            .UseEnvironment(Environments.Development);
    }
}
