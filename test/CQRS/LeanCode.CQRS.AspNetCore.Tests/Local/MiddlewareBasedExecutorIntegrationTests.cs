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

public class MiddlewareBasedExecutorIntegrationTests : IDisposable, IAsyncLifetime
{
    private static readonly TypesCatalog ThisCatalog = TypesCatalog.Of<LocalQuery>();

    private readonly IHost host;
    private readonly TestServer server;

    public MiddlewareBasedExecutorIntegrationTests()
    {
        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddCQRS(ThisCatalog, ThisCatalog).WithLocalQueries(q => { });
                    })
                    .Configure(app =>
                    {
                        app.UseRouting()
                            .UseEndpoints(e =>
                            {
                                e.MapRemoteCQRS("/cqrs", cqrs => { });
                            });
                    });
            })
            .Build();

        server = host.GetTestServer();
    }

    [Fact]
    public async Task Runs_LocalQuery_locally()
    {
        var arg = Guid.NewGuid();

        using var scope = server.Services.CreateScope();

        var executor = scope.ServiceProvider.GetRequiredService<ILocalQueryExecutor>();

        var result = await executor.GetAsync(new LocalQuery(arg), new ClaimsPrincipal());

        result.Should().Be($"local {arg}");
    }

    [Fact]
    public async Task Runs_RemoteQuery_locally()
    {
        var arg = Guid.NewGuid();

        using var scope = server.Services.CreateScope();

        var executor = scope.ServiceProvider.GetRequiredService<ILocalQueryExecutor>();

        var result = await executor.GetAsync(new RemoteQuery(arg), new ClaimsPrincipal());

        result.Should().Be($"remote {arg}: local {arg}");
    }

    [Fact]
    public async Task Runs_LocalQuery_remotely()
    {
        var arg = Guid.NewGuid();

        var result = await HttpGetAsync(new LocalQuery(arg));

        result.Should().Be($"local {arg}");
    }

    [Fact]
    public async Task Runs_RemoteQuery_remotely()
    {
        var arg = Guid.NewGuid();

        var result = await HttpGetAsync(new RemoteQuery(arg));

        result.Should().Be($"remote {arg}: local {arg}");
    }

    protected async Task<string> HttpGetAsync(IQuery<string> query)
    {
        var path = $"/cqrs/query/{query.GetType().FullName}";
        using var msg = new HttpRequestMessage(HttpMethod.Post, path);
        using var content = JsonContent.Create(query, query.GetType(), options: new() { PropertyNamingPolicy = null });
        msg.Content = content;

        var response = await host.GetTestClient().SendAsync(msg);
        return (await response.Content.ReadFromJsonAsync<string>())!;
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

public record LocalQuery(Guid Arg) : IQuery<string>;

public record RemoteQuery(Guid Arg) : IQuery<string>;

public class LocalQueryHandler : IQueryHandler<LocalQuery, string>
{
    public Task<string> ExecuteAsync(HttpContext context, LocalQuery query) => Task.FromResult($"local {query.Arg}");
}

public class RemoteQueryHandler(ILocalQueryExecutor localQuery) : IQueryHandler<RemoteQuery, string>
{
    public async Task<string> ExecuteAsync(HttpContext context, RemoteQuery query)
    {
        var local = await localQuery.GetAsync(new LocalQuery(query.Arg), context.User);
        return $"remote {query.Arg}: {local}";
    }
}
