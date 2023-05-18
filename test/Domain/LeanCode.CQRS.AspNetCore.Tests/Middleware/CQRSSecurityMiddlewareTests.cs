using System.Security.Claims;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

public class CQRSSecurityMiddlewareTests : IAsyncLifetime, IDisposable
{
    private const string SingleAuthorizerCustomData = nameof(SingleAuthorizerCustomData);

    private readonly IHost host;
    private readonly TestServer server;

    private readonly ICustomAuthorizer firstAuthorizer;
    private readonly ICustomAuthorizer secondAuthorizer;

    private static ClaimsPrincipal AuthenticatedUser() => new(new ClaimsIdentity("TEST"));

    public CQRSSecurityMiddlewareTests()
    {
        firstAuthorizer = Substitute.For<ICustomAuthorizer, IFirstAuthorizer>();
        secondAuthorizer = Substitute.For<ICustomAuthorizer, ISecondAuthorizer>();

        host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(cfg =>
                    {
                        cfg.AddSingleton((firstAuthorizer as IFirstAuthorizer)!);
                        cfg.AddSingleton((secondAuthorizer as ISecondAuthorizer)!);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<CQRSSecurityMiddleware>();
                        app.Run(ctx =>
                        {
                            ctx.Response.StatusCode = StatusCodes.Status200OK;
                            return Task.CompletedTask;
                        });
                    });
            })
            .Build();

        server = host.GetTestServer();
    }

    [Fact]
    public async Task Does_not_require_user_authentication_if_object_does_not_have_authorizers()
    {
        var httpContext = await SendPayloadAsync(new NoAuthorization());
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Returns_401NotAuthenticated_if_object_has_authorizers_and_user_is_not_authenticated()
    {
        var httpContext = await SendPayloadAsync(new NoAuthorization());
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Object_with_single_authorizer_authorizes_accordingly(bool isPositive)
    {
        SetAuthorizationResultAsync(firstAuthorizer, isPositive);

        var httpContext = await SendPayloadAsync(new SingleAuthorizer(), AuthenticatedUser());

        var expectedCode = isPositive ? StatusCodes.Status200OK : StatusCodes.Status403Forbidden;
        Assert.Equal(expectedCode, httpContext.Response.StatusCode);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Object_with_multiple_authorizers_authorize_accordingly(bool firstPositive, bool secondPositive)
    {
        SetAuthorizationResultAsync(firstAuthorizer, firstPositive);
        SetAuthorizationResultAsync(secondAuthorizer, secondPositive);

        var httpContext = await SendPayloadAsync(new MultipleAuthorizers(), AuthenticatedUser());

        var expectedCode = firstPositive && secondPositive ? StatusCodes.Status200OK : StatusCodes.Status403Forbidden;
        Assert.Equal(expectedCode, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Throws_if_object_authorizer_is_not_implemented()
    {
        await Assert.ThrowsAsync<CustomAuthorizerNotFoundException>(
            () => SendPayloadAsync(new NotImplementedAuthorizer(), AuthenticatedUser()));
    }

    [Fact]
    public async Task Passes_custom_authorizer_data_to_authorizers()
    {
        var cmd = new SingleAuthorizer();
        var httpContext = await SendPayloadAsync(cmd, AuthenticatedUser());

        await firstAuthorizer.Received().CheckIfAuthorizedAsync(httpContext, cmd, SingleAuthorizerCustomData);
    }

    private Task<HttpContext> SendPayloadAsync(object payload, ClaimsPrincipal? user = null)
    {
        var cqrsMetadata = new CQRSObjectMetadata(
            CQRSObjectKind.Command,
            payload.GetType(),
            typeof(CommandResult),
            typeof(IgnoreType)
        );

        var endpointMetadata = new CQRSEndpointMetadata(cqrsMetadata, (_, __) => Task.FromResult<object?>(null));

        return server.SendAsync(ctx =>
        {
            var endpoint = new Endpoint(null, new EndpointMetadataCollection(endpointMetadata), "Test Endpoint");

            if (user is not null)
            {
                ctx.User = user;
            }

            ctx.Request.Method = "POST";
            ctx.SetEndpoint(endpoint);
            ctx.SetCQRSRequestPayload(payload);
        });
    }

    private static void SetAuthorizationResultAsync(ICustomAuthorizer authorizer, bool result)
    {
        authorizer.CheckIfAuthorizedAsync(null!, null!, null).ReturnsForAnyArgs(result);
    }

    private class NoAuthorization : ICommand { }

    [AuthorizeWhenCustom(typeof(IFirstAuthorizer), SingleAuthorizerCustomData)]
    private class SingleAuthorizer : ICommand { }

    [AuthorizeWhenCustom(typeof(IFirstAuthorizer))]
    [AuthorizeWhenCustom(typeof(ISecondAuthorizer))]
    private class MultipleAuthorizers : ICommand { }

    [AuthorizeWhenCustom(typeof(INotImplementedAuthorizer))]
    private class NotImplementedAuthorizer { }

    // Public, so that NSubstitute could mock it
    public interface IFirstAuthorizer { }
    public interface ISecondAuthorizer { }
    public interface INotImplementedAuthorizer { }

    private sealed class AuthorizeWhenCustomAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenCustomAttribute(Type authorizerType, object? customData = null)
            : base(authorizerType, customData) { }
    }

    private class IgnoreType { }

    public Task InitializeAsync() => host.StartAsync();
    public Task DisposeAsync() => host.StopAsync();
    public void Dispose()
    {
        server.Dispose();
        host.Dispose();
    }
}
