using OpenTelemetry.Trace;

namespace LeanCode.OpenTelemetry;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddLeanCodeTelemetry(this TracerProviderBuilder builder)
    {
        return builder.AddSource(LeanCodeActivitySource.ActivitySource.Name);
    }
}
