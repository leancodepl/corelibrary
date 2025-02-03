using Serilog.Events;

namespace LeanCode.Logging;

/// <summary>
/// Wrapper for Serilog's ILogger, enriched with the type of the class T.
/// </summary>
// ReSharper disable once UnusedTypeParameter
public interface ILogger<T> : Serilog.ILogger { }

public class Logger<T> : ILogger<T>
{
    private readonly Serilog.ILogger logger;

    public Logger()
    {
        logger = Serilog.Log.ForContext<T>();
    }

    public void Write(LogEvent logEvent) => logger.Write(logEvent);
}
