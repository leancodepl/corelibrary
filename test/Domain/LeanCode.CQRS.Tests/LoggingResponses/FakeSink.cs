using System.Globalization;
using Serilog.Core;
using Serilog.Events;

namespace LeanCode.CQRS.Tests;

public class FakeSink : ILogEventSink
{
    private readonly List<string> messages = new();

    public IReadOnlyList<string> Messages
    {
        get
        {
            lock (messages)
            {
                return messages.ToList();
            }
        }
    }

    public void Emit(LogEvent logEvent)
    {
        lock (messages)
        {
            messages.Add(logEvent.RenderMessage(CultureInfo.InvariantCulture));
        }
    }
}
