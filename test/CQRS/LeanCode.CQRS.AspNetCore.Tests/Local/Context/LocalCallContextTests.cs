using System.Security.Claims;
using FluentAssertions;
using LeanCode.CQRS.AspNetCore.Local.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local.Context;

public class LocalCallContextTests
{
    [Fact]
    public void Keeps_user_and_allows_changing_it()
    {
        var user1 = new ClaimsPrincipal();
        var user2 = new ClaimsPrincipal();
        using var context = Create(user: user1);

        context.User.Should().BeSameAs(user1);

        context.User = user2;

        context.User.Should().BeSameAs(user2);
    }

    [Fact]
    public void Keeps_TraceIdentifier_and_allows_changing_it()
    {
        const string Id1 = "id1";
        const string Id2 = "id2";
        using var context = Create(activityId: Id1);

        context.TraceIdentifier.Should().Be(Id1);

        context.TraceIdentifier = Id2;

        context.TraceIdentifier.Should().Be(Id2);
    }

    [Fact]
    public void Provides_items_and_items_feature()
    {
        using var context = Create();
        var feature = context.Features.GetRequiredFeature<IItemsFeature>();

        context.Items.Should().NotBeNull().And.BeSameAs(feature.Items);

        context.Items = new Dictionary<object, object?>();

        context.Items.Should().NotBeNull().And.BeSameAs(feature.Items);
    }

    [Fact]
    public void Keeps_RequestServices_inside_feature()
    {
        var services1 = new ServiceCollection().BuildServiceProvider();
        var services2 = new ServiceCollection().BuildServiceProvider();
        using var context = Create(serviceProvider: services1);
        var feature = context.Features.GetRequiredFeature<IServiceProvidersFeature>();

        context.RequestServices.Should().BeSameAs(services1);
        feature.RequestServices.Should().BeSameAs(services1);

        context.RequestServices = services2;

        context.RequestServices.Should().BeSameAs(services2);
        feature.RequestServices.Should().BeSameAs(services2);
    }

    [Fact]
    public void Keeps_RequestAborted_inside_feature()
    {
        using var context = Create();
        var feature = context.Features.GetRequiredFeature<IHttpRequestLifetimeFeature>();

        context.RequestAborted.Should().Be(feature.RequestAborted);

        using var cts = new CancellationTokenSource();
        context.RequestAborted = cts.Token;

        context.RequestAborted.Should().Be(cts.Token);
        context.RequestAborted.Should().Be(feature.RequestAborted);
    }

    [Fact]
    public void Abort_cancels_both_tokens()
    {
        using var context = Create();

        context.Abort();

        context.RequestAborted.IsCancellationRequested.Should().BeTrue();
        context.CallAborted.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void Connection_is_NullConnectionInfo()
    {
        using var context = Create();

        context.Connection.Should().BeSameAs(NullConnectionInfo.Empty);
    }

    [Fact]
    public void WebSockets_is_NullWebSocketManager()
    {
        using var context = Create();

        context.WebSockets.Should().BeSameAs(NullWebSocketManager.Empty);
    }

    [Fact]
    public void Session_is_NullSession_and_cannot_be_changed()
    {
        using var context = Create();

        context.Session.Should().BeSameAs(NullSession.Empty);

        context.Session = Substitute.For<ISession>();

        context.Session.Should().BeSameAs(NullSession.Empty);
    }

    [Fact]
    public void Response_is_NullHttpResponse()
    {
        using var context = Create();

        context.Response.Should().BeOfType<NullHttpResponse>();
        context.Response.HttpContext.Should().BeSameAs(context);
    }

    [Fact]
    public void Request_is_LocalHttpRequest()
    {
        using var context = Create();

        context.Request.Should().BeOfType<LocalHttpRequest>();
        context.Request.HttpContext.Should().BeSameAs(context);
    }

    [Fact]
    public void Stores_the_passed_headers_in_request()
    {
        var headers = new HeaderDictionary();
        using var context = Create(headers: headers);

        context.Request.Headers.Should().BeSameAs(headers);
    }

    [Fact]
    public void Provides_minimum_set_of_features()
    {
        using var context = Create();

        context.Features.Should().HaveCount(4);
        context.Features.Get<IItemsFeature>().Should().NotBeNull();
        context.Features.Get<IServiceProvidersFeature>().Should().NotBeNull();
        context.Features.Get<IHttpRequestLifetimeFeature>().Should().NotBeNull();
        context.Features.Get<IEndpointFeature>().Should().NotBeNull();
    }

    private static LocalCallContext Create(
        IServiceProvider? serviceProvider = null,
        ClaimsPrincipal? user = null,
        string? activityId = null,
        IHeaderDictionary? headers = null,
        CancellationToken cancellationToken = default
    )
    {
        return new(
            serviceProvider ?? new ServiceCollection().BuildServiceProvider(),
            user ?? new ClaimsPrincipal(),
            activityId,
            headers,
            cancellationToken
        );
    }
}
