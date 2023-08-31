#nullable enable
using System.Security.Claims;
using FluentAssertions;
using LeanCode.CQRS.MassTransitRelay.Middleware;
using LeanCode.IntegrationTestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Middleware;

public sealed class EventsPublisherMiddlewareWithCustomConfigurationTests : EventsPublisherMiddlewareBaseTests
{
    public EventsPublisherMiddlewareWithCustomConfigurationTests()
        : base(cfg =>
        {
            cfg.AddSingleton(new EventsPublisherOptions("other_claim"));
        }) { }

    [Fact]
    public async Task Does_not_propagate_anything_when_the_user_is_unauthorized()
    {
        await Harness.Start();
        await SendAsync(new TestCommand(), null);
        await WaitForProcessing();
        (await Harness.Published.SelectAsync<TestEvent1>().ToListAsync())
            .Should()
            .ContainSingle()
            .Which.Context.Headers.Should()
            .BeEmpty();
    }

    [Fact]
    public async Task Does_not_propagate_anything_in_consumers_when_the_user_is_unauthorized()
    {
        await Harness.Start();
        await SendAsync(new TestCommand(), null);
        await WaitForProcessing();
        (await Harness.Published.SelectAsync<TestEvent2>().ToListAsync())
            .Should()
            .ContainSingle()
            .Which.Context.Headers.Should()
            .BeEmpty();
    }

    [Fact]
    public async Task Propagates_actor_id_in_handlers_if_able()
    {
        await Harness.Start();

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

        await SendAsync(new TestCommand(), testPrincipal);
        await WaitForProcessing();
        (await Harness.Published.SelectAsync<TestEvent1>().ToListAsync())
            .Should()
            .ContainSingle()
            .Which.Context.Headers.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(new KeyValuePair<string, object>("ActorId", "other_claim_value"));
    }

    [Fact]
    public async Task Propagates_actor_id_in_consumers_if_able()
    {
        await Harness.Start();

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

        await SendAsync(new TestCommand(), testPrincipal);
        await WaitForProcessing();
        (await Harness.Published.SelectAsync<TestEvent2>().ToListAsync())
            .Should()
            .ContainSingle()
            .Which.Context.Headers.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(new KeyValuePair<string, object>("ActorId", "other_claim_value"));
    }
}
