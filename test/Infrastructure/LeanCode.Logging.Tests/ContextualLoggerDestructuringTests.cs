using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace LeanCode.Logging.Tests;

public class ContextualLoggerDestructuringTests
{
    private sealed class DestructuringMarker { }

    [Fact]
    public void ContextualLogger_applies_configured_destructuring_policies_to_message_templates()
    {
        var sink = new SingleLogEventCapturerSink();

        using var log = new LoggerConfiguration().Destructure.With<RightSanitizer>().WriteTo.Sink(sink).CreateLogger();

        Logging.ILogger<DestructuringMarker> contextualLogger = new ContextualLogger<DestructuringMarker>(log);

        contextualLogger.Information("{@Payload}", Payload.Workable);

        Assert.NotNull(sink.LastEvent);
        var prop = sink.LastEvent!.Properties["Payload"];
        var rendered = prop.ToString();

        Assert.Contains(BaseSanitizer<Payload>.Placeholder, rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("NOT PLACEHOLDER", rendered, StringComparison.Ordinal);
    }

    private sealed class SingleLogEventCapturerSink : ILogEventSink
    {
        public LogEvent LastEvent { get; private set; } = null!;

        public void Emit(LogEvent logEvent) => LastEvent = logEvent;
    }
}
