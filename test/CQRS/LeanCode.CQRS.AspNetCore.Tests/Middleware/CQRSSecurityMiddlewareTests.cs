using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.Execution;
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

[SuppressMessage(category: "?", "CA1034", Justification = "Nesting public types for better tests separation")]
[SuppressMessage(category: "?", "CA1040", Justification = "Empty marker interfaces")]
public sealed class CQRSSecurityMiddlewareTests : IAsyncLifetime, IDisposable
{
    private const string SingleAuthorizerCustomData = nameof(SingleAuthorizerCustomData);

    private readonly IHost host;
    private readonly TestServer server;

    private readonly ICustomAuthorizer firstAuthorizer;
    private readonly IHttpContextCustomAuthorizer secondAuthorizer;

    private static ClaimsPrincipal AuthenticatedUser() => new(new ClaimsIdentity("TEST"));

    public CQRSSecurityMiddlewareTests()
    {
        firstAuthorizer = Substitute.For<ICustomAuthorizer, IFirstAuthorizer>();
        secondAuthorizer = Substitute.For<IHttpContextCustomAuthorizer, ISecondAuthorizer>();

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
                            var payload = ctx.GetCQRSRequestPayload();
                            payload.SetResult(ExecutionResult.WithPayload(null));
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
        AssertExecutionResult(httpContext, StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Returns_401Unauthorized_if_object_has_authorizers_and_user_is_not_authenticated()
    {
        var httpContext = await SendPayloadAsync(new SingleAuthorizer());
        AssertExecutionResult(httpContext, StatusCodes.Status401Unauthorized);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Object_with_single_authorizer_authorizes_accordingly(bool isPositive)
    {
        SetAuthorizationResultAsync(firstAuthorizer, isPositive);

        var httpContext = await SendPayloadAsync(new SingleAuthorizer(), AuthenticatedUser());

        var expectedCode = isPositive ? StatusCodes.Status200OK : StatusCodes.Status403Forbidden;
        AssertExecutionResult(httpContext, expectedCode);
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
        AssertExecutionResult(httpContext, expectedCode);
    }

    [Fact]
    public async Task Throws_if_object_authorizer_is_not_implemented()
    {
        await Assert.ThrowsAsync<CustomAuthorizerNotFoundException>(
            () => SendPayloadAsync(new NotImplementedAuthorizer(), AuthenticatedUser())
        );
    }

    [Fact]
    public async Task Passes_custom_authorizer_data_to_authorizers()
    {
        var cmd = new SingleAuthorizer();
        var httpContext = await SendPayloadAsync(cmd, AuthenticatedUser());

        await firstAuthorizer
            .Received()
            .CheckIfAuthorizedAsync(Arg.Any<HttpContext>(), cmd, SingleAuthorizerCustomData);
    }

    private Task<HttpContext> SendPayloadAsync(object payload, ClaimsPrincipal? user = null)
    {
        var cqrsMetadata = new CQRSObjectMetadata(
            CQRSObjectKind.Command,
            payload.GetType(),
            typeof(CommandResult),
            typeof(IgnoreType)
        );

        return server.SendAsync(ctx =>
        {
            if (user is not null)
            {
                ctx.User = user;
            }

            ctx.Request.Method = "POST";
            ctx.SetEndpoint(TestHelpers.MockCQRSEndpoint(cqrsMetadata));
            ctx.SetCQRSRequestPayload(payload);
        });
    }

    private static void AssertExecutionResult(HttpContext context, int statusCode)
    {
        var result = context.GetCQRSRequestPayload().Result;

        Assert.NotNull(result);
        Assert.Equal(statusCode, result!.Value.StatusCode);
    }

    private static void SetAuthorizationResultAsync(IHttpContextCustomAuthorizer authorizer, bool result)
    {
        authorizer.CheckIfAuthorizedAsync(null!, null!, null).ReturnsForAnyArgs(result);
    }

    public class NoAuthorization : ICommand { }

    [AuthorizeWhenCustom(typeof(IFirstAuthorizer), SingleAuthorizerCustomData)]
    public class SingleAuthorizer : ICommand { }

    [AuthorizeWhenCustom(typeof(IFirstAuthorizer))]
    [AuthorizeWhenCustom(typeof(ISecondAuthorizer))]
    public class MultipleAuthorizers : ICommand { }

    [AuthorizeWhenCustom(typeof(INotImplementedAuthorizer))]
    private sealed class NotImplementedAuthorizer { }

    // Public, so that NSubstitute could mock it
    public interface IFirstAuthorizer { }

    public interface ISecondAuthorizer { }

    public interface INotImplementedAuthorizer { }

    public sealed class AuthorizeWhenCustomAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenCustomAttribute(Type authorizerType, object? customData = null)
            : base(authorizerType, customData) { }
    }

    public class IgnoreType { }

    public Task InitializeAsync() => host.StartAsync();

    public Task DisposeAsync() => host.StopAsync();

    public void Dispose()
    {
        server.Dispose();
        host.Dispose();
    }
}
