using LeanCode.Components;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LeanCode.CQRS.RemoteHttp.Server;

public static class CQRSEndpointRouteBuilderExtensions
{
    public static void MapRemoteCqrs<TAppContext>(
        this IEndpointRouteBuilder builder,
        string path,
        TypesCatalog catalog,
        Func<HttpContext, TAppContext> contextTranslator,
        ISerializer serializer)
    where TAppContext : class, IPipelineContext
    {
        var handlerBuilder = new ObjectHandlerBuilder<TAppContext>(contextTranslator, serializer);
        var endpointDataSource = new CQRSEndpointsDataSource<TAppContext>(path, catalog, handlerBuilder);

        builder.DataSources.Add(endpointDataSource);
    }
}
