using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Middleware;

[SuppressMessage(category: "?", "CA1034", Justification = "Nesting public types for better tests separation")]
[SuppressMessage(category: "?", "CA1040", Justification = "Empty marker interfaces")]
public sealed class CQRSSecurityMiddlewareTests : CQRSMiddlewareTestBase<CQRSSecurityMiddleware>
{
    private const string SingleAuthorizerCustomData = nameof(SingleAuthorizerCustomData);

    private readonly ICustomAuthorizer firstAuthorizer = Substitute.For<ICustomAuthorizer, IFirstAuthorizer>();
    private readonly IHttpContextCustomAuthorizer secondAuthorizer = Substitute.For<
        IHttpContextCustomAuthorizer,
        ISecondAuthorizer
    >();

    private static ClaimsPrincipal AuthenticatedUser() => new(new ClaimsIdentity("TEST"));

    public CQRSSecurityMiddlewareTests()
    {
        FinalPipeline = ctx =>
        {
            ctx.GetCQRSRequestPayload().SetResult(ExecutionResult.WithPayload(null));
            return Task.CompletedTask;
        };
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_ => (firstAuthorizer as IFirstAuthorizer)!);
        services.AddSingleton(_ => (secondAuthorizer as ISecondAuthorizer)!);
    }

    [Fact]
    public async Task Does_not_require_user_authentication_if_object_does_not_have_authorizers()
    {
        var httpContext = await SendPayloadAsync(new NoAuthorization());
        AssertAuthorizationSuccess(httpContext);
    }

    [Fact]
    public async Task Returns_401Unauthorized_if_object_has_authorizers_and_user_is_not_authenticated()
    {
        var httpContext = await SendPayloadAsync(new SingleAuthorizer());
        AssertAuthorizationFailure(httpContext, StatusCodes.Status401Unauthorized);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Object_with_single_authorizer_authorizes_accordingly(bool isPositive)
    {
        SetAuthorizationResultAsync(firstAuthorizer, isPositive);

        var httpContext = await SendPayloadAsync(new SingleAuthorizer(), AuthenticatedUser());

        if (isPositive)
        {
            AssertAuthorizationSuccess(httpContext);
        }
        else
        {
            AssertAuthorizationFailure(httpContext);
        }
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, true)]
    public async Task Object_with_multiple_authorizers_authorize_accordingly(
        bool firstPositive,
        bool secondPositive,
        bool expectSuccess
    )
    {
        SetAuthorizationResultAsync(firstAuthorizer, firstPositive);
        SetAuthorizationResultAsync(secondAuthorizer, secondPositive);

        var httpContext = await SendPayloadAsync(new MultipleAuthorizers(), AuthenticatedUser());

        if (expectSuccess)
        {
            AssertAuthorizationSuccess(httpContext);
        }
        else
        {
            AssertAuthorizationFailure(httpContext);
        }
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

    private void AssertAuthorizationSuccess(HttpContext context)
    {
        context.ShouldContainExecutionResult(StatusCodes.Status200OK);

        VerifyNoCQRSSuccessMetrics();
        VerifyNoCQRSFailureMetrics();
    }

    private void AssertAuthorizationFailure(HttpContext context, int errorCode = StatusCodes.Status403Forbidden)
    {
        context.ShouldContainExecutionResult(errorCode);
        VerifyCQRSFailureMetrics(CQRSMetrics.AuthorizationFailure, 1);
    }

    private Task<HttpContext> SendPayloadAsync(object payload, ClaimsPrincipal? user = null)
    {
        var cqrsMetadata = new CQRSObjectMetadata(
            CQRSObjectKind.Command,
            payload.GetType(),
            typeof(CommandResult),
            typeof(IgnoreType)
        );

        return Server.SendAsync(ctx =>
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
}
