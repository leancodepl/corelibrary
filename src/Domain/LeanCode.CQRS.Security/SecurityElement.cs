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
            TContext context,
            TInput input,
            Func<TContext, TInput, Task<TOutput>> next)
        {
            var customAuthorizers = AuthorizeWhenAttribute.GetAuthorizers(input);
            var user = context.User;

            if (customAuthorizers.Count > 0)
            {
                if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    throw new UnauthenticatedException(
                        "The current user is not authenticated and the object requires authorization");
                }
            }

            var contextType = context.GetType();
            var objectType = input.GetType();
            foreach (var customAuthorizerDefinition in customAuthorizers)
            {
                var authorizerType = customAuthorizerDefinition.Authorizer;
                var customAuthorizer = authorizerResolver.FindAuthorizer(
                    contextType, authorizerType, objectType);

                if (customAuthorizer == null)
                {
                    throw new CustomAuthorizerNotFoundException(contextType, authorizerType);
                }

                var authorized = await customAuthorizer
                    .CheckIfAuthorized(context, input, customAuthorizerDefinition.CustomData)
                    .ConfigureAwait(false);
                if (!authorized)
                {
                    logger.Warning("Authorizer {Authorizer} failed to authorize the user to run {@Object}",
                        customAuthorizer.UnderlyingAuthorizer.FullName, input);
                    throw new InsufficientPermissionException(
                        $"User is not authorized for {input.GetType()}.");
                }
            }

            return await next(context, input).ConfigureAwait(false);
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
