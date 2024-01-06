using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.AspNetCore.Local;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local;

public class MiddlewareBasedLocalCommandExecutorTests : IDisposable, IAsyncLifetime
{
    private const string IsAuthenticatedHeader = "is-authenticated";
    private static readonly TypesCatalog ThisCatalog = TypesCatalog.Of<LocalCommand>();

    private readonly IHost host;
    private readonly TestServer server;

    public MiddlewareBasedLocalCommandExecutorTests()
    {
        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services
                            .AddCQRS(ThisCatalog, ThisCatalog)
                            .WithLocalCommands(p => p.UseMiddleware<TestMiddleware>());

                        services.AddScoped<TestMiddleware>();
                        services.AddSingleton<DataStorage>();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.Use(MockAuthorization);
                        app.UseEndpoints(e =>
                        {
                            e.MapRemoteCqrs(
                                "/cqrs",
                                cqrs =>
                                {
                                    cqrs.Commands = p => p.UseMiddleware<TestMiddleware>();
                                }
                            );
                        });
                    });
            })
            .Build();

        server = host.GetTestServer();
    }

    [Fact]
    public async Task Runs_both_remote_and_local_commands_successfully()
    {
        var storage = host.Services.GetRequiredService<DataStorage>();
        var result = await SendAsync(new RemoteCommand());

        result.Should().Be(HttpStatusCode.OK);

        storage.RemoteUser.Should().NotBeNullOrEmpty();
        storage.LocalUsers.Should().AllBe(storage.RemoteUser).And.HaveCount(3);

        storage.Middlewares.ToHashSet().Should().HaveSameCount(storage.Middlewares);
        storage.RunHandlers.ToHashSet().Should().HaveSameCount(storage.RunHandlers);
    }

    protected async Task<HttpStatusCode> SendAsync(ICommand cmd, bool isAuthenticated = true)
    {
        var path = $"/cqrs/command/{cmd.GetType().FullName}";
        using var msg = new HttpRequestMessage(HttpMethod.Post, path);
        msg.Content = JsonContent.Create(cmd);
        msg.Headers.Add(IsAuthenticatedHeader, isAuthenticated.ToString());

        var response = await host.GetTestClient().SendAsync(msg);
        return response.StatusCode;
    }

    private static Task MockAuthorization(HttpContext httpContext, RequestDelegate next)
    {
        if (
            httpContext.Request.Headers.TryGetValue(IsAuthenticatedHeader, out var isAuthenticated)
            && isAuthenticated == bool.TrueString
        )
        {
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new("id", Guid.NewGuid().ToString()) }, "Test Identity")
            );
        }

        return next(httpContext);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        server.Dispose();
        host.Dispose();
    }

    public Task InitializeAsync() => host.StartAsync();

    public Task DisposeAsync() => host.StopAsync();
}

public class DataStorage
{
    public string? RemoteUser { get; set; }

    public List<TestMiddleware> Middlewares { get; } = [ ];
    public List<string?> LocalUsers { get; } = [ ];
    public List<LocalCommandHandler> RunHandlers { get; } = [ ];
}

public record RemoteCommand() : ICommand;

public record LocalCommand(string Value, bool Fail) : ICommand;

public class RemoteCommandHandler : ICommandHandler<RemoteCommand>
{
    private readonly DataStorage storage;
    private readonly ILocalCommandExecutor localCommand;

    public RemoteCommandHandler(DataStorage storage, ILocalCommandExecutor localCommand)
    {
        this.storage = storage;
        this.localCommand = localCommand;
    }

    public async Task ExecuteAsync(HttpContext context, RemoteCommand command)
    {
        storage.RemoteUser = context.User?.FindFirst("id")?.Value;

        await localCommand.RunAsync(context, new LocalCommand("Test Val 1", false));
        try
        {
            await localCommand.RunAsync(context, new LocalCommand("Test Val 2", true));
        }
        catch { }
        await localCommand.RunAsync(context, new LocalCommand("Test Val 3", false));
    }
}

public class LocalCommandHandler : ICommandHandler<LocalCommand>
{
    private readonly DataStorage storage;

    public LocalCommand? Command { get; private set; }

    public LocalCommandHandler(DataStorage storage)
    {
        this.storage = storage;
    }

    public async Task ExecuteAsync(HttpContext context, LocalCommand command)
    {
        Command = command;
        storage.RunHandlers.Add(this);
        storage.LocalUsers.Add(context.User?.FindFirst("id")?.Value);

        if (command.Fail)
        {
            throw new InvalidOperationException("Requested.");
        }
    }
}

public class TestMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.RequestServices.GetRequiredService<DataStorage>().Middlewares.Add(this);
        return next(context);
    }
}
