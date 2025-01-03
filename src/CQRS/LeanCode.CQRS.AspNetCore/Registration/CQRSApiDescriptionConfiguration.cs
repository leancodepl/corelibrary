using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Routing;

namespace LeanCode.CQRS.AspNetCore.Registration;

public class CQRSApiDescriptionConfiguration
{
    public Func<RouteEndpoint, CQRSObjectMetadata, IReadOnlyCollection<string>> TagsMapping { get; init; } =
        static (_, _) => Array.Empty<string>();

    public Func<RouteEndpoint, CQRSObjectMetadata, string> SummaryMapping { get; init; } =
        static (_, m) => m.ObjectType.FullName ?? "";

    public Func<RouteEndpoint, CQRSObjectMetadata, string> DescriptionMapping { get; init; } =
        static (_, m) => $"Executed by {m.HandlerType.FullName ?? "unknown handler"}";
}
