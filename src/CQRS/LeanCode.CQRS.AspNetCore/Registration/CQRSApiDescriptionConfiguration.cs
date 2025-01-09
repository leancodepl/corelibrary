using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Routing;

namespace LeanCode.CQRS.AspNetCore.Registration;

public class CQRSApiDescriptionConfiguration
{
    public Func<RouteEndpoint, CQRSObjectMetadata, IReadOnlyList<string>> TagsMapping { get; init; } =
        ApiDescriptionTags.FullNamespace;

    public Func<RouteEndpoint, CQRSObjectMetadata, string> SummaryMapping { get; init; } =
        static (_, m) => m.ObjectType.FullName ?? "";

    public Func<RouteEndpoint, CQRSObjectMetadata, string> DescriptionMapping { get; init; } =
        static (_, m) => $"Executed by {m.HandlerType.FullName ?? "unknown handler"}";
}
