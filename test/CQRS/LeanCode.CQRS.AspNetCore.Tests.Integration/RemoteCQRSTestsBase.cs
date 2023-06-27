using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using LeanCode.Components;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.ForceUpdate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

public abstract class RemoteCQRSTestsBase : IDisposable, IAsyncLifetime
{
    private const string IsAuthenticatedHeader = "is-authenticated";
    protected const string MinimumRequiredVersion = "1.0";
    protected const string CurrentlySupportedVersion = "1.3";

    private readonly IHost host;
    private readonly TestServer server;

    protected RemoteCQRSTestsBase()
    {
        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(cfg =>
                    {
                        cfg.AddScoped<ICustomAuthorizer, CustomAuthorizer>();
                        cfg.AddRouting();
                        cfg.AddCQRS(TypesCatalog.Of<TestCommand>(), TypesCatalog.Of<TestCommandHandler>());
                        cfg.AddForceUpdate();
                        cfg.AddFluentValidation(TypesCatalog.Of<TestCommandValidator>());

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
                        app.Use(MockAuthorization);
                        app.UseEndpoints(ep =>
                        {
                            ep.MapRemoteCqrs(
                                "/cqrs",
                                cqrs =>
                                {
                                    cqrs.Queries = q => q.Secure();
                                    cqrs.Commands = c => c.Secure().Validate();
                                    cqrs.Operations = o => o.Secure();
                                }
                            );
                        });
                    });
            })
            .Build();

        server = host.GetTestServer();
    }

    private static Task MockAuthorization(HttpContext httpContext, RequestDelegate next)
    {
        if (
            httpContext.Request.Headers.TryGetValue(IsAuthenticatedHeader, out var isAuthenticated)
            && isAuthenticated == bool.TrueString
        )
        {
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity("Test Identity"));
        }

        return next(httpContext);
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

        msg.Headers.Add(IsAuthenticatedHeader, isAuthenticated.ToString());

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
