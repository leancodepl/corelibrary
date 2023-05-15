using LeanCode.Components;
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
        Action<CQRSEndpointsBuilder> config
    )
    {
        var cqrsHandlers =
            builder.ServiceProvider.GetService<CQRSObjectsRegistrationSource>()
            ?? throw new InvalidOperationException(
                "CQRS services were not registered, make sure you've called IServiceCollection.AddCQRS(...) first"
            );

        var dataSource = new CQRSEndpointsDataSource(path);
        builder.DataSources.Add(dataSource);

        config(new CQRSEndpointsBuilder(builder, dataSource, cqrsHandlers));
    }
}

public class CQRSEndpointsBuilder
{
    private readonly IEndpointRouteBuilder routeBuilder;
    private readonly CQRSEndpointsDataSource dataSource;
    private readonly CQRSObjectsRegistrationSource cqrsObjectsSource;

    internal CQRSEndpointsBuilder(
        IEndpointRouteBuilder routeBuilder,
        CQRSEndpointsDataSource dataSource,
        CQRSObjectsRegistrationSource cqrsObjectsSource
    )
    {
        this.routeBuilder = routeBuilder;
        this.dataSource = dataSource;
        this.cqrsObjectsSource = cqrsObjectsSource;
    }

    public CQRSEndpointsBuilder WithPipelines<TContext>(
        Func<HttpContext, TContext> contextTranslator,
        Action<IApplicationBuilder> commandsPipeline,
        Action<IApplicationBuilder> queriesPipeline,
        Action<IApplicationBuilder> operationsPipeline
    )
    {
        var objects = cqrsObjectsSource.Objects.Where(obj => obj.ContextType == typeof(TContext));

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
            applicationBuilder.UseMiddleware<CQRSRequestSerializer>(contextTranslator);
            pipelineCfg(applicationBuilder);
            applicationBuilder.Run(CQRSPipelineFinalizer.HandleAsync);
            return applicationBuilder.Build();
        }
    }
}
