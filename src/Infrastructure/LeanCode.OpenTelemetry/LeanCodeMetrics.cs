using System.Diagnostics.Metrics;

namespace LeanCode.OpenTelemetry;

public static class LeanCodeMetrics
{
    public static readonly Meter Meter = new("LeanCode.CoreLibrary");

    public static Counter<T> CreateCounter<T>(string name)
        where T : struct => Meter.CreateCounter<T>(name);
}
