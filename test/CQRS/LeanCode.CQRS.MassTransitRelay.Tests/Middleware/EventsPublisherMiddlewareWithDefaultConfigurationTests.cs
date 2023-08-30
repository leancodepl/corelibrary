#nullable enable
using System.Security.Claims;
using FluentAssertions;
using LeanCode.IntegrationTestHelpers;
using Xunit;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Middleware;

public sealed class EventsPublisherMiddlewareWithDefaultConfigurationTests : EventsPublisherMiddlewareBaseTests
{
    public EventsPublisherMiddlewareWithDefaultConfigurationTests()
        : base(cfg => { }) { }

    [Fact]
    public async Task Does_not_propagate_anything_when_the_user_is_unauthorized()
    {
        await Harness.Start();
        await SendAsync(new TestCommand(), null);
        Harness.Published.Select<TestEvent1>().Should().ContainSingle().Which.Context.Headers.Should().BeEmpty();
    }

    [Fact]
    public async Task Does_not_propagate_anything_in_consumers_when_the_user_is_unauthorized()
    {
        await Harness.Start();
        await SendAsync(new TestCommand(), null);
        Harness.Published.Select<TestEvent2>().Should().ContainSingle().Which.Context.Headers.Should().BeEmpty();
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
        Harness.Published
            .Select<TestEvent1>()
            .Should()
            .ContainSingle()
            .Which.Context.Headers.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(new KeyValuePair<string, object>("ActorId", "test_id"));
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
        Harness.Published
            .Select<TestEvent2>()
            .Should()
            .ContainSingle()
            .Which.Context.Headers.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(new KeyValuePair<string, object>("ActorId", "test_id"));
    }
}
