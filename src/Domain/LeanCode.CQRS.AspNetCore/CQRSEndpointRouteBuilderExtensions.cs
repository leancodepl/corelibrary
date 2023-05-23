using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.AspNetCore.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore;

public static class CQRSEndpointRouteBuilderExtensions
{
    public static void MapRemoteCqrs(
        this IEndpointRouteBuilder builder,
        string path,
        Action<CQRSPipelineBuilder> config
    )
    {
        var cqrsHandlers =
            builder.ServiceProvider.GetService<CQRSObjectsRegistrationSource>()
            ?? throw new InvalidOperationException(
                "CQRS services were not registered, make sure you've called IServiceCollection.AddCQRS(...) first"
            );

        var pipelineBuilder = new CQRSPipelineBuilder(builder);
        config(pipelineBuilder);

        var dataSource = new CQRSEndpointsDataSource(path, new ObjectExecutorFactory());
        dataSource.AddEndpointsFor(
            cqrsHandlers.Objects.Where(pipelineBuilder.ObjectsFilter),
            commandsPipeline: pipelineBuilder.PreparePipeline(pipelineBuilder.Commands),
            queriesPipeline: pipelineBuilder.PreparePipeline(pipelineBuilder.Queries),
            operationsPipeline: pipelineBuilder.PreparePipeline(pipelineBuilder.Operations)
        );
        builder.DataSources.Add(dataSource);
    }
}

public class CQRSPipelineBuilder
{
    public Func<CQRSObjectMetadata, bool> ObjectsFilter { get; set; } = _ => true;

    public Action<IApplicationBuilder> Commands { get; set; } = app => { };
    public Action<IApplicationBuilder> Queries { get; set; } = app => { };
    public Action<IApplicationBuilder> Operations { get; set; } = app => { };

    private readonly IEndpointRouteBuilder routeBuilder;

    internal CQRSPipelineBuilder(IEndpointRouteBuilder routeBuilder)
    {
        this.routeBuilder = routeBuilder;
    }

    internal RequestDelegate PreparePipeline(Action<IApplicationBuilder> pipelineCfg)
    {
        var applicationBuilder = routeBuilder.CreateApplicationBuilder();
        applicationBuilder.UseMiddleware<CQRSRequestSerializer>();
        pipelineCfg(applicationBuilder);
        applicationBuilder.Run(CQRSPipelineFinalizer.HandleAsync);
        return applicationBuilder.Build();
    }
}
