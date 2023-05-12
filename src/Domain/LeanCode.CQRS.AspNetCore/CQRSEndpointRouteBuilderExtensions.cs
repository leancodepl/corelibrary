using LeanCode.Components;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.AspNetCore.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LeanCode.CQRS.AspNetCore;

public static class CQRSEndpointRouteBuilderExtensions
{
    public static void MapRemoteCqrs(
        this IEndpointRouteBuilder builder,
        string path,
        Action<CQRSEndpointsBuilder> config
    )
    {
        var dataSource = new CQRSEndpointsDataSource(path);
        builder.DataSources.Add(dataSource);

        config(new CQRSEndpointsBuilder(builder, dataSource));
    }
}

public class CQRSEndpointsBuilder
{
    private readonly IEndpointRouteBuilder routeBuilder;
    private readonly CQRSEndpointsDataSource dataSource;

    internal CQRSEndpointsBuilder(IEndpointRouteBuilder routeBuilder, CQRSEndpointsDataSource dataSource)
    {
        this.routeBuilder = routeBuilder;
        this.dataSource = dataSource;
    }

    public CQRSEndpointsBuilder WithPipelines<TContext>(
        TypesCatalog contractsCatalog,
        Func<HttpContext, TContext> contextTranslator,
        Action<IApplicationBuilder> commandsPipeline,
        Action<IApplicationBuilder> queriesPipeline,
        Action<IApplicationBuilder> operationsPipeline)
    {
        var objects = AssemblyScanner.FindCqrsObjects(contractsCatalog);

        dataSource.AddEndpointsFor(
            objects,
            new ObjectExecutorFactory<TContext>(),
            commandsPipeline: PreparePipeline(commandsPipeline),
            queriesPipeline: PreparePipeline(queriesPipeline),
            operationsPipeline: PreparePipeline(operationsPipeline)
            );

        return this;

        RequestDelegate PreparePipeline(Action<IApplicationBuilder> pipelineCfg)
        {
            var applicationBuilder = routeBuilder.CreateApplicationBuilder();
            applicationBuilder.UseMiddleware<CQRSPipelineStart>(contextTranslator);
            pipelineCfg(applicationBuilder);
            applicationBuilder.Run(CQRSPipelineFinalizer.HandleAsync);
            return applicationBuilder.Build();
        }
    }
}
