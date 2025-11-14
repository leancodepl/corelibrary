using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using LeanCode.Components;
using LeanCode.CQRS.AspNetCore.Tests.Integration.OutputCache;
using LeanCode.CQRS.OutputCaching;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.Logging.AspNetCore;
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

    private readonly IHost host;
    private readonly TestServer server;

    protected int minimalApiInvocationCount = 0;

    protected RemoteCQRSTestsBase(bool enableMinimalApi = false)
    {
        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(cfg =>
                    {
                        cfg.AddScoped<ICustomAuthorizer, CustomAuthorizer>();
                        cfg.AddScoped<IHttpContextCustomAuthorizer, HttpContextCustomAuthorizer>();
                        cfg.AddRouting();
                        cfg.AddCQRS(TypesCatalog.Of<TestCommand>(), TypesCatalog.Of<TestCommandHandler>());
                        cfg.AddFluentValidation(TypesCatalog.Of<TestCommandValidator>());
                        cfg.AddCQRSOutputCache(TypesCatalog.Of<CachedTestQueryPolicy>());
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.Use(MockAuthorization);
                        app.UseWhen(
                            ctx =>
                                enableMinimalApi
                                && !ctx.Request.Path.StartsWithSegments(
                                    "/cqrs",
                                    StringComparison.InvariantCultureIgnoreCase
                                ),
                            app => app.UseOutputCache()
                        );
                        app.UseEndpoints(ep =>
                        {
                            ep.MapRemoteCQRS(
                                "/cqrs",
                                cqrs =>
                                {
                                    cqrs.Queries = q => q.Secure().CacheOutput();
                                    cqrs.Commands = c => c.Secure().Validate();
                                    cqrs.Operations = o => o.Secure().CacheOutput();
                                }
                            );

                            if (enableMinimalApi)
                            {
                                ep.MapGet("/invocation-count", () => $"{++minimalApiInvocationCount}").CacheOutput();
                            }
                        });
                    });
            })
            .ConfigureDefaultLogging("test")
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

    protected async Task<Response> SendAsync(
        string path,
        string body = "{}",
        HttpMethod? method = null,
        bool isAuthenticated = true,
        Dictionary<string, string>? headers = null
    )
    {
        method ??= HttpMethod.Post;

        using var msg = new HttpRequestMessage(method, path);
        if (method == HttpMethod.Post)
        {
            msg.Content = new StringContent(body, new MediaTypeHeaderValue("application/json", "utf-8"));
        }

        msg.Headers.Add(IsAuthenticatedHeader, isAuthenticated.ToString());

        if (headers != null)
        {
            foreach (var (key, value) in headers)
            {
                msg.Headers.TryAddWithoutValidation(key, value);
            }
        }

        var response = await host.GetTestClient().SendAsync(msg);

        var responseBody = await response.Content.ReadAsStringAsync();
        return new Response(responseBody, response.StatusCode, response.Headers);
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

    public async ValueTask InitializeAsync() => await host.StartAsync();

    public async ValueTask DisposeAsync() => await host.StopAsync();

    protected record struct Response(string Body, HttpStatusCode StatusCode, HttpResponseHeaders Headers);
}
