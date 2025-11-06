using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.AspNetCore.Middleware;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.Logging;
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
        FinalPipeline = ctx => ctx.CompleteCQRSExecutionResult(ExecutionResult.WithPayload(null));
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_ => (firstAuthorizer as IFirstAuthorizer)!);
        services.AddSingleton(_ => (secondAuthorizer as ISecondAuthorizer)!);
        services.AddLogging(logging => logging.AddNullLeanCodeLogger());
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
            VerifyActivity(
                $"middleware - Security - {typeof(IFirstAuthorizer).FullName}",
                activityStatusCode: ActivityStatusCode.Ok
            );
        }
        else
        {
            AssertAuthorizationFailure(httpContext);
            VerifyActivity($"middleware - Security - {typeof(IFirstAuthorizer).FullName}");
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
            VerifyActivity(
                $"middleware - Security - {typeof(IFirstAuthorizer).FullName}",
                activityStatusCode: ActivityStatusCode.Ok
            );
            VerifyActivity(
                $"middleware - Security - {typeof(ISecondAuthorizer).FullName}",
                activityStatusCode: ActivityStatusCode.Ok
            );
        }
        else
        {
            AssertAuthorizationFailure(httpContext);
            VerifyActivity("middleware - Security");
        }
    }

    [Fact]
    public async Task Throws_if_object_authorizer_is_not_implemented()
    {
        await Assert.ThrowsAsync<CustomAuthorizerNotFoundException>(
            () => SendPayloadAsync(new NotImplementedAuthorizer(), AuthenticatedUser())
        );
        VerifyActivity($"middleware - Security - {typeof(INotImplementedAuthorizer).FullName}");
    }

    [Fact]
    public async Task Passes_custom_authorizer_data_to_authorizers()
    {
        var cmd = new SingleAuthorizer();
        await SendPayloadAsync(cmd, AuthenticatedUser());

        await firstAuthorizer
            .Received()
            .CheckIfAuthorizedAsync(Arg.Any<HttpContext>(), cmd, SingleAuthorizerCustomData);
    }

    private void AssertAuthorizationSuccess(HttpContext context)
    {
        context
            .ShouldHaveResponseStatusCode(StatusCodes.Status200OK)
            .ShouldHaveResponseContentType(Serializer.ContentType)
            .ShouldContainExecutionResult(StatusCodes.Status200OK);

        VerifyNoCQRSSuccessMetrics();
        VerifyNoCQRSFailureMetrics();
    }

    private void AssertAuthorizationFailure(HttpContext context, int errorCode = StatusCodes.Status403Forbidden)
    {
        context.ShouldHaveResponseStatusCode(errorCode).ShouldContainExecutionResult(errorCode);

        VerifyCQRSFailureMetrics(CQRSMetrics.AuthorizationFailure, 1);
    }

    private Task<HttpContext> SendPayloadAsync(object payload, ClaimsPrincipal? user = null)
    {
        var cqrsMetadata = new CQRSObjectMetadata(
            CQRSObjectKind.Command,
            payload.GetType(),
            typeof(CommandResult),
            typeof(IgnoreType),
            (_, _) => Task.FromResult<object?>(null)
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

    private sealed class NoAuthorization : ICommand;

    [AuthorizeWhenCustom(typeof(IFirstAuthorizer), SingleAuthorizerCustomData)]
    private sealed class SingleAuthorizer : ICommand;

    [AuthorizeWhenCustom(typeof(IFirstAuthorizer))]
    [AuthorizeWhenCustom(typeof(ISecondAuthorizer))]
    private sealed class MultipleAuthorizers : ICommand;

    [AuthorizeWhenCustom(typeof(INotImplementedAuthorizer))]
    private sealed class NotImplementedAuthorizer;

    // Public, so that NSubstitute could mock it
    public interface IFirstAuthorizer : ICustomAuthorizer;

    public interface ISecondAuthorizer : ICustomAuthorizer;

    public interface INotImplementedAuthorizer;

    public sealed class AuthorizeWhenCustomAttribute : AuthorizeWhenAttribute
    {
        public AuthorizeWhenCustomAttribute(Type authorizerType, object? customData = null)
            : base(authorizerType, customData) { }
    }

    private sealed class IgnoreType;
}
