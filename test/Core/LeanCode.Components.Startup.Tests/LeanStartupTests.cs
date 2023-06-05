using LeanCode.Components.Autofac;
using LeanCode.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;

namespace LeanCode.Startup.Autofac.Tests;

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

        Assert.Same(Log.Logger, Serilog.Core.Logger.None);
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

        Assert.NotSame(Log.Logger, Serilog.Core.Logger.None);
    }
}

public class TestStartupWithTrueParameter : LeanStartup
{
    protected override IReadOnlyList<IAppModule> Modules { get; }

    public TestStartupWithTrueParameter(IConfiguration config)
        : base(config)
    {
        Modules = Array.Empty<IAppModule>();
    }

    protected override void ConfigureApp(IApplicationBuilder app) { }
}

public class TestStartupWithFalseParameter : LeanStartup
{
    protected override IReadOnlyList<IAppModule> Modules { get; }
    protected override bool CloseAndFlushLogger { get; }

    public TestStartupWithFalseParameter(IConfiguration config)
        : base(config)
    {
        Modules = Array.Empty<IAppModule>();
    }

    protected override void ConfigureApp(IApplicationBuilder app) { }
}
