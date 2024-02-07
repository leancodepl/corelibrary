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

public class MiddlewareBasedLocalCommandExecutor : ILocalCommandExecutor
{
    private readonly IServiceProvider serviceProvider;
    private readonly ICQRSObjectSource objectSource;

    private readonly RequestDelegate pipeline;

    public MiddlewareBasedLocalCommandExecutor(
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

    public Task<CommandResult> RunAsync<T>(
        T command,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    )
        where T : ICommand => RunInternalAsync(command, user, null, cancellationToken);

    public Task<CommandResult> RunAsync<T>(
        T command,
        ClaimsPrincipal user,
        IHeaderDictionary headers,
        CancellationToken cancellationToken = default
    )
        where T : ICommand => RunInternalAsync(command, user, headers, cancellationToken);

    private async Task<CommandResult> RunInternalAsync<T>(
        T command,
        ClaimsPrincipal user,
        IHeaderDictionary? headers,
        CancellationToken cancellationToken
    )
        where T : ICommand
    {
        var metadata = objectSource.MetadataFor(typeof(T));

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

        localContext.SetCQRSRequestPayload(command);
        localContext.SetCQRSObjectMetadataForLocalExecution(metadata);

        await pipeline(localContext);

        localContext.CallAborted.ThrowIfCancellationRequested();

        return (CommandResult)localContext.GetCQRSRequestPayload().Result!.Value.Payload!;
    }
}
