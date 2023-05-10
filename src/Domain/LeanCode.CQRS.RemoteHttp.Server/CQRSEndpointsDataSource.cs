using System.Reflection;
using LeanCode.Components;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace LeanCode.CQRS.RemoteHttp.Server;

public class CQRSEndpointsDataSource<TContext> : EndpointDataSource
where TContext : IPipelineContext
{
    private static readonly MethodInfo CommandBuilder = typeof(ObjectHandlerBuilder<TContext>).GetMethod("Command")!;
    private static readonly MethodInfo QueryBuilder = typeof(ObjectHandlerBuilder<TContext>).GetMethod("Query")!;
    private static readonly MethodInfo OperationBuilder = typeof(ObjectHandlerBuilder<TContext>).GetMethod("Operation")!;

    private readonly RoutePattern basePath;
    private readonly TypesCatalog catalog;
    private readonly ObjectHandlerBuilder<TContext> handlerBuilder;

    public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;
    public override IReadOnlyList<Endpoint> Endpoints { get; }

    public CQRSEndpointsDataSource(string basePath, TypesCatalog catalog, ObjectHandlerBuilder<TContext> handlerBuilder)
    {
        this.basePath = RoutePatternFactory.Pattern(basePath);
        this.catalog = catalog;
        this.handlerBuilder = handlerBuilder;

        Endpoints = BuildEndpoints();
    }

    private IReadOnlyList<Endpoint> BuildEndpoints()
    {
        var result = new List<Endpoint>();

        foreach (var obj in ObjectsFinder.FindCqrsObjects(catalog))
        {
            var builder = new RouteEndpointBuilder(
                MakeHandler(obj),
                ObjectRoute(obj),
                0);

            builder.DisplayName = obj.ObjectType.FullName;

            result.Add(builder.Build());
        }

        return result;
    }

    private RoutePattern ObjectRoute(CQRSObjectMetadata obj)
    {
        var commandPrefix = RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart("command"));
        var typePart = RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart(obj.ObjectType.FullName!));

        var path = RoutePatternFactory.Pattern(commandPrefix, typePart);

        return RoutePatternFactory.Combine(basePath, path);
    }

    private RequestDelegate MakeHandler(CQRSObjectMetadata @object)
    {
        var builderMethod = CommandBuilder.MakeGenericMethod(@object.ObjectType);
        return (RequestDelegate)builderMethod.Invoke(handlerBuilder, Array.Empty<object?>())!;
    }
}
