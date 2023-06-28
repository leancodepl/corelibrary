using System.Net;
using System.Net.Http.Headers;
using LeanCode.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LeanCode.CQRS.AspNetCore;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LeanCode.ForceUpdate.Tests;

public abstract class ForceUpdateTestsBase : IDisposable, IAsyncLifetime
{
    private readonly IHost host;
    private readonly TestServer server;

    protected const string MinimumRequiredVersion = "1.0";
    protected const string CurrentlySupportedVersion = "1.3";

    protected ForceUpdateTestsBase()
    {
        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(cfg =>
                    {
                        cfg.AddRouting();
                        cfg.AddCQRS(TypesCatalog.Of<Query1>(), TypesCatalog.Of<Query1Handler>()).AddForceUpdate();
                        cfg.AddSingleton(
                            new IOSVersionsConfiguration(
                                new Version(MinimumRequiredVersion),
                                new Version(CurrentlySupportedVersion)
                            )
                        );
                        cfg.AddSingleton(
                            new AndroidVersionsConfiguration(
                                new Version(MinimumRequiredVersion),
                                new Version(CurrentlySupportedVersion)
                            )
                        );
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(ep =>
                        {
                            ep.MapRemoteCqrs(
                                "/cqrs",
                                cqrs =>
                                {
                                    cqrs.Queries = q => q.Secure();
                                }
                            );
                        });
                    });
            })
            .Build();

        server = host.GetTestServer();
    }

    private class Query1 : IQuery<QueryResult1> { }

    private class QueryResult1 { }

    private class Query1Handler : IQueryHandler<Query1, QueryResult1>
    {
        public Task<QueryResult1> ExecuteAsync(HttpContext context, Query1 query) =>
            throw new NotImplementedException();
    }

    protected async Task<(string ReponseBody, HttpStatusCode StatusCode)> SendAsync(
        string path,
        string body = "{}",
        HttpMethod? method = null,
        bool isAuthenticated = true
    )
    {
        method ??= HttpMethod.Post;

        using var msg = new HttpRequestMessage(method, path);
        if (method == HttpMethod.Post)
        {
            msg.Content = new StringContent(body, new MediaTypeHeaderValue("application/json", "utf-8"));
        }

        var response = await host.GetTestClient().SendAsync(msg);

        var responseBody = await response.Content.ReadAsStringAsync();
        return (responseBody, response.StatusCode);
    }

    public void Dispose()
    {
        server.Dispose();
        host.Dispose();
    }

    public Task InitializeAsync() => host.StartAsync();

    public Task DisposeAsync() => host.StopAsync();
}
