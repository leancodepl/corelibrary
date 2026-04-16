using System.Diagnostics.CodeAnalysis;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace LeanCode.Logging;

public class ContextualLogger<T>(ILogger logger) : ILogger<T>
{
    private readonly ILogger logger = logger.ForContext<T>();

    public void Write(LogEvent logEvent) => logger.Write(logEvent);

    public bool BindMessageTemplate(
        string messageTemplate,
        object?[]? propertyValues,
        [NotNullWhen(true)] out MessageTemplate? parsedTemplate,
        [NotNullWhen(true)] out IEnumerable<LogEventProperty>? boundProperties
    ) => logger.BindMessageTemplate(messageTemplate, propertyValues, out parsedTemplate, out boundProperties);
}
