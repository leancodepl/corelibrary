using System.Diagnostics;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.Logging;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSSecurityMiddleware
{
    private readonly CQRSMetrics metrics;
    private readonly RequestDelegate next;
    private readonly ILogger<CQRSSecurityMiddleware> logger;

    public CQRSSecurityMiddleware(CQRSMetrics metrics, RequestDelegate next, ILogger<CQRSSecurityMiddleware> logger)
    {
        this.metrics = metrics;
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var activity = LeanCodeActivitySource.StartMiddleware("Security");
        var cqrsMetadata = context.GetCQRSObjectMetadata();
        var payload = context.GetCQRSRequestPayload();

        var customAuthorizers = AuthorizeWhenAttribute.GetCustomAuthorizers(cqrsMetadata.ObjectType);

        if (customAuthorizers.Count == 0)
        {
            activity?.SetTag("security.authorizers_present", false);
            await next(context);
            return;
        }
        activity?.SetTag("security.authorizers_present", true);

        if (!(context.User.Identity?.IsAuthenticated ?? false))
        {
            activity?.SetTag("security.user_authenticated", false);
            metrics.CQRSFailure(CQRSMetrics.AuthorizationFailure);
            logger.Warning(
                "The current user is not authenticated and the object {@Object} requires authorization",
                payload.Payload
            );

            context.SetCQRSExecutionResult(ExecutionResult.Empty(StatusCodes.Status401Unauthorized));
            return;
        }
        activity?.SetTag("security.user_authenticated", true);

        foreach (var customAuthorizerDefinition in customAuthorizers)
        {
            var authorizerType = customAuthorizerDefinition.Authorizer;
            using var authorizerActivity = LeanCodeActivitySource.StartMiddleware("Security", authorizerType.FullName);
            authorizerActivity?.SetTag("security.authorizer.type", authorizerType.FullName);

            var customAuthorizer =
                context.RequestServices.GetService(authorizerType) as IHttpContextCustomAuthorizer
                ?? throw new CustomAuthorizerNotFoundException(authorizerType);

            var authorized = await customAuthorizer.CheckIfAuthorizedAsync(
                context,
                payload.Payload,
                customAuthorizerDefinition.CustomData
            );

            if (!authorized)
            {
                authorizerActivity?.SetTag("security.authorizer.authorized", false);
                metrics.CQRSFailure(CQRSMetrics.AuthorizationFailure);
                logger.Warning(
                    "User is not authorized for {@Object}, authorizer {AuthorizerType} did not pass",
                    payload.Payload,
                    customAuthorizer.GetType().FullName
                );

                context.SetCQRSExecutionResult(ExecutionResult.Empty(StatusCodes.Status403Forbidden));
                return;
            }
            else
            {
                authorizerActivity?.SetStatus(ActivityStatusCode.Ok);
                authorizerActivity?.SetTag("security.authorizer.authorized", true);
            }
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        await next(context);
    }
}
