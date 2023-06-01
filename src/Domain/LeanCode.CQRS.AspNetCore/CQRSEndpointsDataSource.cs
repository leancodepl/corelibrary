using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace LeanCode.CQRS.AspNetCore;

internal class CQRSEndpointsDataSource : EndpointDataSource
{
    private readonly IObjectExecutorFactory executorFactory;
    private readonly RoutePattern basePath;
    private readonly List<Endpoint> endpoints = new();

    public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;

    public override IReadOnlyList<Endpoint> Endpoints => endpoints;

    public CQRSEndpointsDataSource(string basePath, IObjectExecutorFactory executorFactory)
    {
        this.executorFactory = executorFactory;
        this.basePath = RoutePatternFactory.Parse(basePath);
    }

    public void AddEndpointsFor(
        IEnumerable<CQRSObjectMetadata> objects,
        RequestDelegate commandsPipeline,
        RequestDelegate queriesPipeline,
        RequestDelegate operationsPipeline
    )
    {
        foreach (var obj in objects)
        {
            var builder = new RouteEndpointBuilder(PipelineFor(obj), RouteFor(obj), 0)
            {
                DisplayName = $"{obj.ObjectKind} {obj.ObjectType.FullName}",
                Metadata =
                {
                    new CQRSEndpointMetadata(obj, executorFactory.CreateExecutorFor(obj)),
                    new HttpMethodMetadata(new[] { HttpMethods.Post })
                }
            };

            endpoints.Add(builder.Build());
        }

        RequestDelegate PipelineFor(CQRSObjectMetadata obj)
        {
            return obj.ObjectKind switch
            {
                CQRSObjectKind.Command => commandsPipeline,
                CQRSObjectKind.Query => queriesPipeline,
                CQRSObjectKind.Operation => operationsPipeline,
                _ => throw new InvalidOperationException($"Unexpected object kind: {obj.ObjectKind}")
            };
        }
    }

    private RoutePattern RouteFor(CQRSObjectMetadata obj)
    {
        var kindSegment = obj.ObjectKind switch
        {
            CQRSObjectKind.Command => RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart("command")),
            CQRSObjectKind.Query => RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart("query")),
            CQRSObjectKind.Operation => RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart("operation")),
            _ => throw new InvalidOperationException($"Unexpected object kind: {obj.ObjectKind}")
        };

        var typeSegment = RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart(obj.ObjectType.FullName!));
        var path = RoutePatternFactory.Pattern(kindSegment, typeSegment);
        return RoutePatternFactory.Combine(basePath, path);
    }
}
