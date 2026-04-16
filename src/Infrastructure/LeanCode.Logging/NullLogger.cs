using System.Diagnostics.CodeAnalysis;
using Serilog.Events;
using Serilog.Parsing;

namespace LeanCode.Logging;

public class NullLogger<T> : ILogger<T>
{
    public static readonly NullLogger<T> Instance = new();

    public void Write(LogEvent logEvent) { }

    public bool BindMessageTemplate(
        string messageTemplate,
        object?[]? propertyValues,
        [NotNullWhen(true)] out MessageTemplate? parsedTemplate,
        [NotNullWhen(true)] out IEnumerable<LogEventProperty>? boundProperties
    )
    {
        parsedTemplate = null;
        boundProperties = null;
        return false;
    }
}
