using LeanCode.Contracts.Security;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
        var cqrsMetadata = context.GetCQRSObjectMetadata();
        var payload = context.GetCQRSRequestPayload();

        var customAuthorizers = AuthorizeWhenAttribute.GetCustomAuthorizers(cqrsMetadata.ObjectType);
        var user = context.User;

        if (customAuthorizers.Count > 0 && !(user.Identity?.IsAuthenticated ?? false))
        {
            logger.LogWarning(
                "The current user is not authenticated and the object {@Object} requires authorization",
                payload.Payload
            );

            payload.SetResult(ExecutionResult.Empty(StatusCodes.Status401Unauthorized));
            metrics.CQRSFailure(CQRSMetrics.AuthorizationFailure);
            return;
        }

        foreach (var customAuthorizerDefinition in customAuthorizers)
        {
            var authorizerType = customAuthorizerDefinition.Authorizer;
            using var activity = LeanCodeActivitySource.StartMiddleware("Security", authorizerType.FullName);
            activity?.SetTag("authorizer.type", authorizerType.FullName);

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
                activity?.SetTag("authorizer.authorized", false);
                logger.LogWarning(
                    "User is not authorized for {@Object}, authorizer {AuthorizerType} did not pass",
                    payload.Payload,
                    customAuthorizer.GetType().FullName
                );

                payload.SetResult(ExecutionResult.Empty(StatusCodes.Status403Forbidden));
                metrics.CQRSFailure(CQRSMetrics.AuthorizationFailure);
                return;
            }
            else
            {
                activity?.SetTag("authorizer.authorized", true);
            }
        }

        await next(context);
    }
}
