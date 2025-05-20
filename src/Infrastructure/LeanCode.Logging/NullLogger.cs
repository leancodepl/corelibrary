using Serilog.Events;

namespace LeanCode.Logging;

public class NullLogger<T> : ILogger<T>
{
    public static readonly NullLogger<T> Instance = new();

    public void Write(LogEvent logEvent) { }
}
