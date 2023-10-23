using System.Collections;
using System.Globalization;
using ConfigCat.Client;
using Microsoft.Extensions.Logging;
using CCLogLevel = ConfigCat.Client.LogLevel;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace LeanCode.ConfigCat;

// https://github.com/configcat/.net-sdk/blob/57f08f27508e6f98c6e4db5ca305dfa4ade36b0f/samples/ASP.NETCore/WebApplication/Adapters/ConfigCatToMSLoggerAdapter.cs
public class ConfigCatToMSLoggerAdapter : IConfigCatLogger
{
    private readonly ILogger logger;

    public ConfigCatToMSLoggerAdapter(ILogger<ConfigCatClient> logger)
    {
        this.logger = logger;
    }

    // Allow all log levels here and let MS logger do log level filtering (see appsettings.json)
    public CCLogLevel LogLevel
    {
        get => CCLogLevel.Debug;
        set { }
    }

    public void Log(
        CCLogLevel level,
        LogEventId eventId,
        ref FormattableLogMessage message,
        Exception? exception = null
    )
    {
        var logLevel = level switch
        {
            CCLogLevel.Error => MSLogLevel.Error,
            CCLogLevel.Warning => MSLogLevel.Warning,
            CCLogLevel.Info => MSLogLevel.Information,
            CCLogLevel.Debug => MSLogLevel.Debug,
            _ => MSLogLevel.None
        };

        var logValues = new LogValues(ref message);

        logger.Log(
            logLevel,
            eventId.Id,
            state: logValues,
            exception,
            static (state, _) => state.Message.InvariantFormattedMessage
        );

        message = logValues.Message;
    }

    // Support for structured logging.
    private sealed class LogValues : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private string? templateFormat;

        public LogValues(ref FormattableLogMessage message)
        {
            Message = message;
        }

        public FormattableLogMessage Message { get; private set; }

        public KeyValuePair<string, object?> this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfNegative(index);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

                return index == Count - 1
                    ? new("{OriginalFormat}", templateFormat ??= RenderTemplate())
                    : new(Message.ArgNames[index], Message.ArgValues[index]);
            }
        }

        public int Count => (Message.ArgNames?.Length ?? 0) + 1;

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (int i = 0, n = Count; i < n; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => Message.InvariantFormattedMessage;

        private string RenderTemplate()
        {
            // Message.Format contains template in a format: 'A sample error message with parameter value: {0}'
            // Serilog does not treat it as valid argument, and logs this as is.
            // So instead we render the format to get: 'A sample error message with parameter value: {Value}'
            // Which can be consumed correctly later
            return Message.ArgNames is { } argNames
                ? string.Format(
                    provider: CultureInfo.InvariantCulture,
                    format: Message.Format,
                    args: argNames.Select(a => $"{{{a}}}" as object).ToArray()
                )
                : Message.Format;
        }
    }
}
