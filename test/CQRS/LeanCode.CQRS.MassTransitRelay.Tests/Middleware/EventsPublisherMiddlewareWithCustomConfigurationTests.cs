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

public sealed class EventsPublisherMiddlewareWithCustomConfigurationTests : IDisposable, IAsyncLifetime
{
    private readonly IHost host;
    private readonly TestServer server;
    private readonly IServiceProvider serviceProvider;
    private readonly ITestHarness harness;

    public EventsPublisherMiddlewareWithCustomConfigurationTests()
    {
        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(cfg =>
                    {
                        cfg.AddAuthentication().AddTestAuthenticationHandler();
                        cfg.AddMassTransitTestHarness();
                        cfg.AddAsyncEventsInterceptor();

                        cfg.AddSingleton(new EventsPublisherOptions("other_claim"));
                    })
                    .Configure(app =>
                    {
                        app.UseAuthentication();
                        app.UseMiddleware<EventsPublisherMiddleware>();
                        app.Run(ctx =>
                        {
                            var payload = ctx.GetCQRSRequestPayload();
                            DomainEvents.Raise(new TestEvent());
                            payload.SetResult(Execution.ExecutionResult.WithPayload(CommandResult.Success));
                            return Task.CompletedTask;
                        });
                    });
            })
            .Build();

        server = host.GetTestServer();
        serviceProvider = server.Services;
        harness = serviceProvider.GetRequiredService<ITestHarness>();

        harness.TestTimeout = TimeSpan.FromSeconds(1);
    }

    [Fact]
    public async Task Does_not_propagate_anything_when_the_user_is_unauthorized()
    {
        await harness.Start();
        var ctx = await SendAsync(new TestCommand(), null);
        AssertCommandResultSuccess(ctx);
        harness.Published.Select<TestEvent>().Should().ContainSingle().Which.Context.Headers.Should().BeEmpty();
    }

    [Fact]
    public async Task Propagates_actor_id_if_able()
    {
        await harness.Start();

        var testPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new("sub", "test_id"),
                    new("role", "user"),
                    new("role", "admin"),
                    new("other_claim", "other_claim_value")
                },
                TestAuthenticationHandler.SchemeName,
                "sub",
                "role"
            )
        );

        var ctx = await SendAsync(new TestCommand(), testPrincipal);
        AssertCommandResultSuccess(ctx);
        var tmp = harness.Published
            .Select<TestEvent>()
            .Should()
            .ContainSingle()
            .Which.Context.Headers.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(new KeyValuePair<string, object>("ActorId", "other_claim_value"));
    }

    private static void AssertCommandResultSuccess(HttpContext httpContext)
    {
        var rawResult = httpContext.GetCQRSRequestPayload().Result?.Payload;

        var commandResult = rawResult.Should().BeOfType<CommandResult>().Subject;
        commandResult.WasSuccessful.Should().BeTrue();
        commandResult.ValidationErrors.Should().BeEmpty();
    }

    private Task<HttpContext> SendAsync(ICommand command, ClaimsPrincipal? principal)
    {
        return server.SendAsync(ctx =>
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

    public Task DisposeAsync()
    {
        return Task.WhenAll(host.StopAsync(), harness.Stop());
    }

    public void Dispose()
    {
        server.Dispose();
        host.Dispose();
    }

    public Task InitializeAsync() => host.StartAsync();
}
