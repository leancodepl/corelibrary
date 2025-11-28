using System.Diagnostics.Metrics;
using LeanCode.OpenTelemetry;

namespace LeanCode.CQRS.OutputCaching;

public static class CQRSOutputCacheMetrics
{
    private static readonly Meter Meter = new(LeanCodeMetrics.MeterName);
    public static readonly Counter<int> CqrsOutputCacheHit = Meter.CreateCounter<int>("cqrs.output_cache.hit");
    public static readonly Counter<int> CqrsOutputCacheMiss = Meter.CreateCounter<int>("cqrs.output_cache.miss");

    public static void CacheHit() => CqrsOutputCacheHit.Add(1);

    public static void CacheMiss() => CqrsOutputCacheMiss.Add(1);
}
