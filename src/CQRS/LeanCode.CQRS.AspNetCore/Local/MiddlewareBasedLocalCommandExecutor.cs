using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore.Local;

public class MiddlewareBasedLocalCommandExecutor : ILocalCommandExecutor
{
    private readonly ICQRSObjectSource objectSource;

    private readonly RequestDelegate pipeline;

    public MiddlewareBasedLocalCommandExecutor(
        IServiceProvider serviceProvider,
        ICQRSObjectSource objectSource,
        Action<ICQRSApplicationBuilder> configure
    )
    {
        this.objectSource = objectSource;

        var app = new CQRSApplicationBuilder(new ApplicationBuilder(serviceProvider));
        configure(app);
        app.Run(CQRSPipelineFinalizer.HandleAsync);
        pipeline = app.Build();
    }

    public async Task<CommandResult> RunAsync<T>(
        HttpContext context,
        T command,
        CancellationToken cancellationToken = default
    )
        where T : ICommand
    {
        await using var scope = context.RequestServices.CreateAsyncScope();

        var metadata = objectSource.MetadataFor(typeof(T));
        var localContext = new LocalHttpContext(context, scope.ServiceProvider, cancellationToken);

        localContext.SetCQRSRequestPayload(command);
        localContext.SetCQRSObjectMetadataForLocalExecution(metadata);
        localContext.Features.Set<IEndpointFeature>(new StubEndpointFeature());

        await pipeline(localContext);

        cancellationToken.ThrowIfCancellationRequested();

        return (CommandResult)localContext.GetCQRSRequestPayload().Result!.Value.Payload!;
    }
}

internal class StubEndpointFeature : IEndpointFeature
{
    public Endpoint? Endpoint
    {
        get => null;
        set { }
    }
}
