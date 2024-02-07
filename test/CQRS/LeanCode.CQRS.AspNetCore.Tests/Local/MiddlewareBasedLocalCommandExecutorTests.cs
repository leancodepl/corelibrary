using System.Security.Claims;
using FluentAssertions;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Local;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local;

public class MiddlewareBasedLocalCommandExecutorTests
{
    private static readonly TypesCatalog ThisCatalog = TypesCatalog.Of<LocalCommand>();

    private readonly LocalDataStorage storage = new();
    private readonly IServiceProvider serviceProvider;
    private readonly ICQRSObjectSource objectSource;

    private readonly MiddlewareBasedLocalCommandExecutor executor;

    public MiddlewareBasedLocalCommandExecutorTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(storage);
        serviceCollection.AddScoped<IMiddlewareFactory>(sp => new MiddlewareFactory(sp));
        serviceCollection.AddScoped<LocalHandlerMiddleware>();

        var registrationSource = new CQRSObjectsRegistrationSource(serviceCollection, new ObjectExecutorFactory());
        registrationSource.AddCQRSObjects(ThisCatalog, ThisCatalog);

        serviceProvider = serviceCollection.BuildServiceProvider();
        objectSource = registrationSource;

        executor = new(serviceProvider, objectSource, app => app.UseMiddleware<LocalHandlerMiddleware>());
    }

    [Fact]
    public async Task Runs_the_command()
    {
        var command = new LocalCommand();
        var result = await executor.RunAsync(command, new ClaimsPrincipal());

        result.Should().Be(CommandResult.Success);

        storage.Commands.Should().Contain(command);
        storage.Handlers.Should().ContainSingle();
    }

    [Fact]
    public async Task Runs_the_middleware()
    {
        var command = new LocalCommand();
        await executor.RunAsync(command, new ClaimsPrincipal());

        storage.Middlewares.Should().ContainSingle();
    }

    [Fact]
    public async Task Each_run_opens_separate_DI_scope()
    {
        var command1 = new LocalCommand();
        var command2 = new LocalCommand();
        await executor.RunAsync(command1, new ClaimsPrincipal());
        await executor.RunAsync(command2, new ClaimsPrincipal());

        storage.Commands.Should().BeEquivalentTo([ command1, command2 ]);
        storage.Handlers.Should().HaveCount(2);
        storage.Handlers.Should().HaveCount(2);
    }

    [Fact]
    public async Task Exceptions_are_not_catched()
    {
        var command = new LocalCommand(Fail: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => executor.RunAsync(command, new ClaimsPrincipal()));
    }

    [Fact]
    public async Task Calls_can_be_aborted()
    {
        var command = new LocalCommand(Cancel: true);

        await Assert.ThrowsAsync<OperationCanceledException>(() => executor.RunAsync(command, new ClaimsPrincipal()));
    }

    [Fact]
    public async Task Object_metadata_is_set()
    {
        var command = new LocalCommand(CheckMetadata: true);

        await executor.RunAsync(command, new ClaimsPrincipal());
    }
}

public class LocalDataStorage
{
    public List<LocalHandlerMiddleware> Middlewares { get; } = [ ];
    public List<LocalCommandHandler> Handlers { get; } = [ ];
    public List<LocalCommand> Commands { get; } = [ ];
}

public record LocalCommand(bool Fail = false, bool Cancel = false, bool CheckMetadata = false) : ICommand;

public class LocalCommandHandler : ICommandHandler<LocalCommand>
{
    private readonly LocalDataStorage storage;

    public LocalCommandHandler(LocalDataStorage storage)
    {
        this.storage = storage;
    }

    public Task ExecuteAsync(HttpContext context, LocalCommand command)
    {
        storage.Commands.Add(command);
        storage.Handlers.Add(this);

        if (command.Fail)
        {
            throw new InvalidOperationException("Requested.");
        }

        if (command.Cancel)
        {
            context.Abort();
        }

        if (command.CheckMetadata)
        {
            context.GetCQRSObjectMetadata().ObjectKind.Should().Be(CQRSObjectKind.Command);
            context.GetCQRSObjectMetadata().ObjectType.Should().Be(typeof(LocalCommand));
            context.GetCQRSObjectMetadata().HandlerType.Should().Be(typeof(LocalCommandHandler));
        }

        return Task.CompletedTask;
    }
}

public class LocalHandlerMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.RequestServices.GetRequiredService<LocalDataStorage>().Middlewares.Add(this);
        return next(context);
    }
}
