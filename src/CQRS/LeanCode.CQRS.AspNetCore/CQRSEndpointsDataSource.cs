using System.Reflection;
using LeanCode.CQRS.Annotations;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace LeanCode.CQRS.AspNetCore;

internal class CQRSEndpointsDataSource : EndpointDataSource
{
    private readonly RoutePattern basePath;
    private readonly List<Endpoint> endpoints = [];
    private readonly IEnumerable<ICQRSEndpointMetadataProvider> endpointMetadataProviders;

    public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;

    public override IReadOnlyList<Endpoint> Endpoints => endpoints;

    public CQRSEndpointsDataSource(
        string basePath,
        IEnumerable<ICQRSEndpointMetadataProvider> endpointMetadataProviders
    )
    {
        this.basePath = RoutePatternFactory.Parse(basePath);
        this.endpointMetadataProviders = endpointMetadataProviders;
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
            var pipeline = PipelineFor(obj);
            var httpMetadata = new HttpMethodMetadata([HttpMethods.Post]);

            foreach (var route in RoutesFor(obj))
            {
                var endpoint = new RouteEndpoint(
                    pipeline,
                    route.Pattern,
                    0,
                    BuildMetadata(obj, httpMetadata),
                    $"{obj.ObjectKind} {route.Name}"
                );
                endpoints.Add(endpoint);
            }
        }

        RequestDelegate PipelineFor(CQRSObjectMetadata obj)
        {
            return obj.ObjectKind switch
            {
                CQRSObjectKind.Command => commandsPipeline,
                CQRSObjectKind.Query => queriesPipeline,
                CQRSObjectKind.Operation => operationsPipeline,
                _ => throw new InvalidOperationException($"Unexpected object kind: {obj.ObjectKind}"),
            };
        }
    }

    private EndpointMetadataCollection BuildMetadata(CQRSObjectMetadata metadata, HttpMethodMetadata httpMetadata)
    {
        return new([
            metadata,
            httpMetadata,
            .. endpointMetadataProviders.SelectMany(e => e.GetAdditionalEndpointMetadata(metadata)),
        ]);
    }

    private IEnumerable<(string Name, RoutePattern Pattern)> RoutesFor(CQRSObjectMetadata obj)
    {
        var kindString = obj.ObjectKind switch
        {
            CQRSObjectKind.Command => "command",
            CQRSObjectKind.Query => "query",
            CQRSObjectKind.Operation => "operation",
            _ => throw new InvalidOperationException($"Unexpected object kind: {obj.ObjectKind}"),
        };
        var kindSegment = RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart(kindString));

        var typeName = obj.ObjectType.FullName!;

        yield return (typeName, Path(typeName));

        foreach (var alias in obj.ObjectType.GetCustomAttributes<PathAliasAttribute>())
        {
            yield return (alias.Path, Path(alias.Path));
        }

        RoutePattern Path(string name)
        {
            var typeSegment = RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart(name));
            var path = RoutePatternFactory.Pattern($"{kindString}/{name}", kindSegment, typeSegment);
            return RoutePatternFactory.Combine(basePath, path);
        }
    }
}
