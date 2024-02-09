using System.Security.Claims;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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

    protected async Task<TResult> RunInternalAsync<TResult>(
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

        return Decode<TResult>(obj, localContext.GetCQRSRequestPayload().Result!.Value);
    }

    private static T Decode<T>(object payload, ExecutionResult result)
    {
        return result.StatusCode switch
        {
            StatusCodes.Status200OK or StatusCodes.Status422UnprocessableEntity => (T)result.Payload!,
            StatusCodes.Status401Unauthorized => throw new UnauthenticatedCQRSRequestException(payload.GetType()),
            StatusCodes.Status403Forbidden => throw new UnauthorizedCQRSRequestException(payload.GetType()),
            var e => throw new UnknownStatusCodeException(e, payload.GetType()),
        };
    }
}
