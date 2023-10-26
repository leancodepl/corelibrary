using System.Diagnostics.Metrics;
using LeanCode.OpenTelemetry;

namespace LeanCode.CQRS.AspNetCore;

internal class CQRSMetrics
{
    private static readonly Counter<int> cqrsSuccess = LeanCodeMetrics.CreateCounter<int>("cqrs.success");
    private static readonly Counter<int> cqrsFailure = LeanCodeMetrics.CreateCounter<int>("cqrs.failure");

    public static void CQRSSuccess() => cqrsSuccess.Add(1);

    public static void CQRSFailure(string reason) =>
        cqrsFailure.Add(1, KeyValuePair.Create("reason", reason as object)!);

    public const string SerializationFailure = "serialization";
    public const string AuthorizationFailure = "authorization";
    public const string ValidationFailure = "validation";
    public const string InternalError = "internal_error";
}
