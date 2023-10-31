using OpenTelemetry.Metrics;

namespace LeanCode.OpenTelemetry;

public static class MeterBuilderExtensions
{
    public static MeterProviderBuilder AddLeanCodeMetrics(this MeterProviderBuilder builder)
    {
        return builder.AddMeter(LeanCodeMetrics.MeterName);
    }
}
