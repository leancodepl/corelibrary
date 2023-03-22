using LeanCode.Components;
using LeanCode.Components.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LeanCode.Startup.Tests;

public class LeanStartupTests
{
    [Fact]
    public async Task Sink_closes_when_we_start_the_app_with_closeAndFlushLogger_parameter_set_as_true()
    {
        var host = LeanProgram.BuildMinimalHost<TestStartupWithTrueParameter>().ConfigureDefaultLogging("test").Build();

        await host.StartAsync();

        var lifetime = host.Services.GetService<IHostApplicationLifetime>();
        lifetime.StopApplication();

        await host.WaitForShutdownAsync();

        var logger = host.Services.GetService<ILogger<TestStartupWithTrueParameter>>();

        var isSinkClosed =
            !logger.IsEnabled(LogLevel.Debug)
            && !logger.IsEnabled(LogLevel.Information)
            && !logger.IsEnabled(LogLevel.Warning)
            && !logger.IsEnabled(LogLevel.Error)
            && !logger.IsEnabled(LogLevel.Critical);

        Assert.True(isSinkClosed);
    }

    [Fact]
    public async Task Sink_does_not_close_when_we_start_the_app_with_closeAndFlushLogger_parameter_set_as_false()
    {
        var host = LeanProgram
            .BuildMinimalHost<TestStartupWithFalseParameter>()
            .ConfigureDefaultLogging("test")
            .Build();

        await host.StartAsync();

        var lifetime = host.Services.GetService<IHostApplicationLifetime>();
        lifetime.StopApplication();

        await host.WaitForShutdownAsync();

        var logger = host.Services.GetService<ILogger<TestStartupWithFalseParameter>>();

        var isSinkClosed =
            !logger.IsEnabled(LogLevel.Debug)
            && !logger.IsEnabled(LogLevel.Information)
            && !logger.IsEnabled(LogLevel.Warning)
            && !logger.IsEnabled(LogLevel.Error)
            && !logger.IsEnabled(LogLevel.Critical);

        Assert.False(isSinkClosed);
    }
}

public class TestStartupWithTrueParameter : LeanStartup
{
    protected override IReadOnlyList<IAppModule> Modules { get; }

    public TestStartupWithTrueParameter(IConfiguration config)
        : base(config, true)
    {
        Modules = new IAppModule[] { };
    }

    protected override void ConfigureApp(IApplicationBuilder app) { }
}

public class TestStartupWithFalseParameter : LeanStartup
{
    protected override IReadOnlyList<IAppModule> Modules { get; }

    public TestStartupWithFalseParameter(IConfiguration config)
        : base(config, false)
    {
        Modules = new IAppModule[] { };
    }

    protected override void ConfigureApp(IApplicationBuilder app) { }
}
