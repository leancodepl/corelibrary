using Serilog;
using Serilog.Events;

namespace LeanCode.Logging;

public class ContextualLogger<T>(ILogger logger) : ILogger<T>
{
    private readonly ILogger logger = logger.ForContext<T>();

    public void Write(LogEvent logEvent) => logger.Write(logEvent);
}
