#nullable enable
using System.Net.Http.Headers;
using System.Security.Claims;
using FluentAssertions;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.MassTransitRelay.Middleware;
using LeanCode.DomainModels.Model;
using LeanCode.IntegrationTestHelpers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Middleware;

public class EventsPublisherMiddlewareBaseTests : IDisposable, IAsyncLifetime
{
    protected IHost Host { get; private init; }
    protected TestServer Server { get; private set; }
    protected IServiceProvider ServiceProvider { get; private set; }
    protected ITestHarness Harness { get; private set; }

    public EventsPublisherMiddlewareBaseTests(Action<IServiceCollection> customConfiguration)
    {
        Host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(cfg =>
                    {
                        cfg.AddAuthentication().AddTestAuthenticationHandler();
                        cfg.AddMassTransitTestHarness(c =>
                        {
                            c.AddConsumer(typeof(TestEventConsumer), typeof(TestEventConsumerDefinition));
                        });
                        cfg.AddAsyncEventsInterceptor();
                        customConfiguration(cfg);
                    })
                    .Configure(app =>
                    {
                        app.UseAuthentication();
                        app.UseMiddleware<EventsPublisherMiddleware>();
                        app.Run(ctx =>
                        {
                            var payload = ctx.GetCQRSRequestPayload();
                            DomainEvents.Raise(new TestEvent1());
                            payload.SetResult(Execution.ExecutionResult.WithPayload(CommandResult.Success));
                            return Task.CompletedTask;
                        });
                    });
            })
            .Build();
        Server = null!;
        ServiceProvider = null!;
        Harness = null!;
    }

    protected Task<HttpContext> SendAsync(ICommand command, ClaimsPrincipal? principal)
    {
        return Server.SendAsync(ctx =>
        {
            if (principal is not null)
            {
                ctx.Request.Headers.Authorization = new AuthenticationHeaderValue(
                    TestAuthenticationHandler.SchemeName,
                    TestAuthenticationHandler.SerializePrincipal(principal)
                ).ToString();
            }

            var cqrsMetadata = new CQRSObjectMetadata(
                CQRSObjectKind.Command,
                command.GetType(),
                typeof(CommandResult),
                typeof(IgnoreType)
            );

            ctx.SetEndpoint(TestHelpers.MockCQRSEndpoint(cqrsMetadata));
            ctx.SetCQRSRequestPayload(command);
        });
    }

    protected async Task WaitForProcessing()
    {
        // There appears to be some hole in the test harness when tested with outbox.
        // Even though the bus has stabilized, harness does not see the messages published.
        // Resorting to harness inactivity which will takes more time, but is not flaky.
        await Harness.InactivityTask;
    }

    public Task DisposeAsync()
    {
        return Task.WhenAll(Host.StopAsync(), Harness.Stop());
    }

    public void Dispose()
    {
        Server.Dispose();
        Host.Dispose();
    }

    public async Task InitializeAsync()
    {
        await Host.StartAsync();
        Server = Host.GetTestServer();
        ServiceProvider = Server.Services;
        Harness = ServiceProvider.GetRequiredService<ITestHarness>();

        Harness.TestTimeout = TimeSpan.FromSeconds(1);
    }
}
