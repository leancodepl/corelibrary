using Serilog.Events;

namespace LeanCode.Logging;

public interface ILogger<T> : Serilog.ILogger { }

public class Logger<T> : ILogger<T>
{
    private readonly Serilog.ILogger logger;

    public Logger(Serilog.ILogger logger)
    {
        this.logger = logger.ForContext<T>();
    }

    public void Write(LogEvent logEvent) => logger.Write(logEvent);
}
