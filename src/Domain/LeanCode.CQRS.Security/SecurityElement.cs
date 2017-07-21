using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security.Exceptions;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Security
{
    public class SecurityElement<TInput, TOutput>
        : IPipelineElement<ExecutionContext, TInput, TOutput>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SecurityElement<TInput, TOutput>>();

        // This is bad, we need to switch to context-based information that is
        // passed by the caller of executor and do not rely on HttpContextAccessor.
        // That way we may be able to use the CQRS infrastructure outside of HTTP
        // context (async command queues) and avoid many errors related to not-so-perfect
        // human memory (e.g. latest change - handling unauthorized users correctly)
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAuthorizerResolver authorizerResolver;

        public SecurityElement(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizerResolver authorizerResolver)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.authorizerResolver = authorizerResolver;
        }

        public async Task<TOutput> ExecuteAsync(
            ExecutionContext ctx,
            TInput input,
            Func<ExecutionContext, TInput, Task<TOutput>> next)
        {
            var customAuthorizers = AuthorizeWhenAttribute.GetAuthorizers(input);

            if (customAuthorizers.Count > 0)
            {
                var curr = httpContextAccessor.HttpContext;
                if (curr.User == null || curr.User.Identity == null || !curr.User.Identity.IsAuthenticated)
                {
                    throw new UnauthenticatedException(
                        "The current user is not authenticated and the object requires authorization");
                }
            }

            foreach (var customAuthorizerDefinition in customAuthorizers)
            {
                var customAuthorizer = authorizerResolver.FindAuthorizer(customAuthorizerDefinition.Authorizer);
                var authorized = await customAuthorizer
                    .CheckIfAuthorized(input, customAuthorizerDefinition.CustomData)
                    .ConfigureAwait(false);
                if (!authorized)
                {
                    logger.Warning("Authorizer {Authorizer} failed to authorize the user to run {@Object}",
                        customAuthorizer.GetType().FullName, input);
                    throw new InsufficientPermissionException(
                        $"User is not authorized for {input.GetType()}.");
                }
            }

            return await next(ctx, input).ConfigureAwait(false);
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<ExecutionContext, TInput, TOutput> Secure<TInput, TOutput>(
            this PipelineBuilder<ExecutionContext, TInput, TOutput> builder
        )
        {
            return builder.Use<SecurityElement<TInput, TOutput>>();
        }
    }
}
