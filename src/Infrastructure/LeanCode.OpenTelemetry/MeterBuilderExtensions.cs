using OpenTelemetry.Metrics;

namespace LeanCode.OpenTelemetry;

public static class MeterBuilderExtensions
{
    public static MeterProviderBuilder AddLeanCodeInstrumentation(this MeterProviderBuilder builder)
    {
        return builder.AddMeter(LeanCodeMetrics.MeterName);
    }

    [Obsolete($"Use {nameof(AddLeanCodeInstrumentation)} instead.")]
    public static MeterProviderBuilder AddLeanCodeMetrics(this MeterProviderBuilder builder)
    {
        return builder.AddLeanCodeInstrumentation();
    }
}
