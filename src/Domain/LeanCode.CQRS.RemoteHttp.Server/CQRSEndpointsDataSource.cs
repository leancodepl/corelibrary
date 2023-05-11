using LeanCode.Components;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace LeanCode.CQRS.RemoteHttp.Server;

internal class CQRSEndpointsDataSource<TContext> : EndpointDataSource
where TContext : IPipelineContext
{
    private readonly RoutePattern basePath;
    private readonly TypesCatalog catalog;
    private readonly CQRSRequestDelegate<TContext> requestDelegate;
    private readonly IObjectExecutorFactory executorFactory;

    public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;
    public override IReadOnlyList<Endpoint> Endpoints { get; }

    public CQRSEndpointsDataSource(
        string basePath,
        TypesCatalog catalog,
        CQRSRequestDelegate<TContext> requestDelegate,
        IObjectExecutorFactory executorFactory)
    {
        this.basePath = RoutePatternFactory.Parse(basePath);
        this.catalog = catalog;
        this.requestDelegate = requestDelegate;
        this.executorFactory = executorFactory;

        Endpoints = BuildEndpoints();
    }

    private IReadOnlyList<Endpoint> BuildEndpoints()
    {
        var result = new List<Endpoint>();

        foreach (var obj in AssemblyScanner.FindCqrsObjects(catalog))
        {
            var builder = new RouteEndpointBuilder(
                requestDelegate.HandleAsync,
                RouteFor(obj),
                0)
            {
                DisplayName = $"{obj.ObjectKind} {obj.ObjectType.FullName}",
                Metadata =
                {
                    new CQRSEndpointMetadata(obj, executorFactory.CreateExecutorFor(obj)),
                    new HttpMethodMetadata(new [] { HttpMethods.Post })
                }
            };

            result.Add(builder.Build());
        }

        return result;
    }

    private RoutePattern RouteFor(CQRSObjectMetadata obj)
    {
        var kindSegment = obj.ObjectKind switch
        {
            CQRSObjectKind.Command => RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart("command")),
            CQRSObjectKind.Query => RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart("query")),
            CQRSObjectKind.Operation => RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart("operation")),
            _ => throw new InvalidOperationException()
        };

        var typeSegment = RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart(obj.ObjectType.FullName!));
        var path = RoutePatternFactory.Pattern(kindSegment, typeSegment);
        return RoutePatternFactory.Combine(basePath, path);
    }
}
