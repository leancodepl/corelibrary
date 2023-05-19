using LeanCode.Contracts.Security;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSSecurityMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger logger = Log.ForContext<CQRSSecurityMiddleware>();

    public CQRSSecurityMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cqrsMetadata = context.GetCQRSEndpoint().ObjectMetadata;
        var payload = context.GetCQRSRequestPayload();

        var customAuthorizers = AuthorizeWhenAttribute.GetCustomAuthorizers(cqrsMetadata.ObjectType);
        var user = context.User;

        if (customAuthorizers.Count > 0 && !(user?.Identity?.IsAuthenticated ?? false))
        {
            logger.Warning("The current user is not authenticated and the object requires authorization");

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        foreach (var customAuthorizerDefinition in customAuthorizers)
        {
            var authorizerType = customAuthorizerDefinition.Authorizer;
            var customAuthorizer = context.RequestServices.GetService(authorizerType) as ICustomAuthorizer;

            if (customAuthorizer is null)
            {
                throw new CustomAuthorizerNotFoundException(authorizerType);
            }

            var authorized = await customAuthorizer.CheckIfAuthorizedAsync(
                context,
                payload.Payload,
                customAuthorizerDefinition.CustomData
            );

            if (!authorized)
            {
                logger.Warning("User is not authorized for {ObjectType}, authorizer {AuthorizerType} did not pass",
                    cqrsMetadata.ObjectType.FullName,
                    customAuthorizer.GetType().FullName
                    );

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        await next(context);
    }
}
