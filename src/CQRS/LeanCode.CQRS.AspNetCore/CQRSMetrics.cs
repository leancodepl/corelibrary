using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using LeanCode.OpenTelemetry;

namespace LeanCode.CQRS.AspNetCore;

public class CQRSMetrics
{
    public const string SerializationFailure = "serialization";
    public const string AuthorizationFailure = "authorization";
    public const string ValidationFailure = "validation";
    public const string InternalError = "internal_error";

    private readonly Counter<int> cqrsSuccess;
    private readonly Counter<int> cqrsFailure;

    [SuppressMessage("?", "CA2000", Justification = "Meter lifetime if managed by DI and it doesn't need to be disposed manually")]
    public CQRSMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(LeanCodeMetrics.MeterName);

        cqrsSuccess = meter.CreateCounter<int>("cqrs.success");
        cqrsFailure = meter.CreateCounter<int>("cqrs.failure");
    }

    public void CQRSSuccess() => cqrsSuccess.Add(1);

    public void CQRSFailure(string reason) => cqrsFailure.Add(1, KeyValuePair.Create("reason", reason as object)!);
}
