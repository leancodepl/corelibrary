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
        ISerializer serializer
    )
        where TAppContext : class, IPipelineContext
    {
        var requestDelegate = new CQRSRequestDelegate<TAppContext>(serializer, contextTranslator);
        var executorFactory = new ObjectExecutorFactory<TAppContext>();
        var endpointDataSource = new CQRSEndpointsDataSource<TAppContext>(
            path,
            catalog,
            requestDelegate,
            executorFactory
        );

        builder.DataSources.Add(endpointDataSource);
    }
}
