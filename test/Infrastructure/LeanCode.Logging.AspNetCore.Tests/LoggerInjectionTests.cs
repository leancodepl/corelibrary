using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace LeanCode.Logging.AspNetCore.Tests;

public class LoggerInjectionTests
{
    private static readonly SingleLogEventCapturerSink Sink = new();

    private readonly IServiceProvider serviceProvider;

    public LoggerInjectionTests()
    {
        serviceProvider = new ServiceCollection()
            .AddLogging(logging =>
                Log.Logger = logging.AddLeanCodeLogging(
                    new ConfigurationBuilder().Build(),
                    "test",
                    "test",
                    formatConsoleLogsAsJson: true,
                    destructurers: [typeof(LoggerInjectionTests).Assembly],
                    additionalLoggingConfiguration: lc => lc.WriteTo.Sink(Sink)
                )
            )
            .BuildServiceProvider(validateScopes: true);
    }

    [Fact]
    public void Logger_can_be_resolved_from_the_container_in_unscoped_contexts()
    {
        var logger = serviceProvider.GetService<ILogger<LoggerInjectionTests>>();

        Assert.NotNull(logger);
    }

    [Fact]
    public void Logger_can_be_resolved_from_the_container_in_scoped_contexts()
    {
        using var scope = serviceProvider.CreateScope();

        var logger = scope.ServiceProvider.GetService<ILogger<LoggerInjectionTests>>();

        Assert.NotNull(logger);
    }

    [Fact]
    public void Resolved_and_static_loggers_write_to_the_same_sinks()
    {
        var injectedLogger = serviceProvider.GetRequiredService<ILogger<LoggerInjectionTests>>();
        var staticLogger = Log.ForContext<LoggerInjectionTests>();

        injectedLogger.Information("One");

        var evt = Sink.LogEvent.Value;

        Assert.Equal(LogEventLevel.Information, evt.Level);
        Assert.Equal("One", evt.MessageTemplate.Text);

        staticLogger.Warning("Two");

        evt = Sink.LogEvent.Value;

        Assert.Equal(LogEventLevel.Warning, evt.Level);
        Assert.Equal("Two", evt.MessageTemplate.Text);

        injectedLogger.Error("Three");

        evt = Sink.LogEvent.Value;

        Assert.Equal(LogEventLevel.Error, evt.Level);
        Assert.Equal("Three", evt.MessageTemplate.Text);
    }

    internal sealed class SingleLogEventCapturerSink : ILogEventSink
    {
        public ThreadLocal<LogEvent> LogEvent { get; private set; } = new();

        public void Emit(LogEvent logEvent) => LogEvent.Value = logEvent;
    }
}
