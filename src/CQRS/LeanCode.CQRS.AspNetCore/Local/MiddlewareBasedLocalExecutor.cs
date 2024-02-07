using System.Security.Claims;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore.Local;

public abstract class MiddlewareBasedLocalExecutor
{
    private readonly IServiceProvider serviceProvider;
    private readonly ICQRSObjectSource objectSource;

    private readonly RequestDelegate pipeline;

    protected MiddlewareBasedLocalExecutor(
        IServiceProvider serviceProvider,
        ICQRSObjectSource objectSource,
        Action<ICQRSApplicationBuilder> configure
    )
    {
        this.serviceProvider = serviceProvider;
        this.objectSource = objectSource;

        var app = new CQRSApplicationBuilder(new ApplicationBuilder(serviceProvider));
        configure(app);
        app.Run(CQRSPipelineFinalizer.HandleAsync);
        pipeline = app.Build();
    }

    protected async Task<object?> RunInternalAsync(
        object obj,
        ClaimsPrincipal user,
        IHeaderDictionary? headers,
        CancellationToken cancellationToken
    )
    {
        var metadata = objectSource.MetadataFor(obj.GetType());

        using var activity = LeanCodeActivitySource.ActivitySource.StartActivity("pipeline.action.local");
        activity?.AddTag("object", metadata.ObjectType.FullName);

        await using var scope = serviceProvider.CreateAsyncScope();

        using var localContext = new Context.LocalCallContext(
            scope.ServiceProvider,
            user,
            activity?.Id,
            headers,
            cancellationToken
        );

        localContext.SetCQRSRequestPayload(obj);
        localContext.SetCQRSObjectMetadataForLocalExecution(metadata);

        await pipeline(localContext);

        localContext.CallAborted.ThrowIfCancellationRequested();

        return localContext.GetCQRSRequestPayload().Result!.Value.Payload;
    }
}
