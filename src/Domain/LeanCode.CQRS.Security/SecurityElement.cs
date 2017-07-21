using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Security.Exceptions;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Security
{
    public class SecurityElement<TContext, TInput, TOutput>
        : IPipelineElement<TContext, TInput, TOutput>
        where TContext : ISecurityContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SecurityElement<TContext, TInput, TOutput>>();

        private readonly IAuthorizerResolver authorizerResolver;

        public SecurityElement(IAuthorizerResolver authorizerResolver)
        {
            this.authorizerResolver = authorizerResolver;
        }

        public async Task<TOutput> ExecuteAsync(
            TContext ctx,
            TInput input,
            Func<TContext, TInput, Task<TOutput>> next)
        {
            var customAuthorizers = AuthorizeWhenAttribute.GetAuthorizers(input);
            var user = ctx.User;

            if (customAuthorizers.Count > 0)
            {
                if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    throw new UnauthenticatedException(
                        "The current user is not authenticated and the object requires authorization");
                }
            }

            foreach (var customAuthorizerDefinition in customAuthorizers)
            {
                var customAuthorizer = authorizerResolver.FindAuthorizer(customAuthorizerDefinition.Authorizer);
                var authorized = await customAuthorizer
                    .CheckIfAuthorized(user, input, customAuthorizerDefinition.CustomData)
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
        public static PipelineBuilder<TContext, TInput, TOutput> Secure<TContext, TInput, TOutput>(
            this PipelineBuilder<TContext, TInput, TOutput> builder)
            where TContext : ISecurityContext
        {
            return builder.Use<SecurityElement<TContext, TInput, TOutput>>();
        }
    }
}
