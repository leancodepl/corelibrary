using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore.Local;

public class MiddlewareBasedLocalCommandExecutor : ILocalCommandExecutor
{
    private readonly IServiceProvider serviceProvider;

    private RequestDelegate? pipeline;

    public MiddlewareBasedLocalCommandExecutor(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void Configure(Action<ICQRSApplicationBuilder> configure)
    {
        var app = new CQRSApplicationBuilder(new ApplicationBuilder(serviceProvider));
        configure(app);
        app.Run(LocalCommandExecutor.HandleAsync);
        pipeline = app.Build();
    }

    public async Task<CommandResult> RunAsync<T>(
        HttpContext context,
        T command,
        CancellationToken cancellationToken = default
    )
        where T : ICommand
    {
        if (pipeline is null)
        {
            throw new InvalidOperationException("`Configure` first.");
        }

        await using var scope = serviceProvider.CreateAsyncScope();

        var localContext = new LocalHttpContext(context, scope.ServiceProvider, cancellationToken);

        localContext.SetCQRSRequestPayload(command);
        localContext.Features.Set<ILocalCommandPayload>(new LocalCommandPayload<T>(command));

        await pipeline(localContext);

        cancellationToken.ThrowIfCancellationRequested();

        return (CommandResult)localContext.GetCQRSRequestPayload().Result!.Value.Payload!;
    }
}

internal class LocalCommandExecutor
{
    public static Task HandleAsync(HttpContext context)
    {
        var localPayload = context.Features.GetRequiredFeature<ILocalCommandPayload>();
        return localPayload.ExecuteAsync(context);
    }
}

internal interface ILocalCommandPayload
{
    Task ExecuteAsync(HttpContext context);
}

internal class LocalCommandPayload<T> : ILocalCommandPayload
    where T : ICommand
{
    private readonly T cmd;

    public LocalCommandPayload(T cmd)
    {
        this.cmd = cmd;
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        var payload = context.GetCQRSRequestPayload();
        await context.RequestServices.GetRequiredService<ICommandHandler<T>>().ExecuteAsync(context, cmd);
        payload.SetResult(ExecutionResult.WithPayload(CommandResult.Success));
    }
}
