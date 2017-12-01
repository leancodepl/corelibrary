using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security.Exceptions;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Security
{
    public class CQRSSecurityElement<TAppContext, TInput, TOutput>
        : IPipelineElement<TAppContext, TInput, TOutput>
        where TAppContext : ISecurityContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CQRSSecurityElement<TAppContext, TInput, TOutput>>();

        private readonly IAuthorizerResolver<TAppContext> authorizerResolver;

        public CQRSSecurityElement(IAuthorizerResolver<TAppContext> authorizerResolver)
        {
            this.authorizerResolver = authorizerResolver;
        }

        public async Task<TOutput> ExecuteAsync(
            TAppContext appContext,
            TInput input,
            Func<TAppContext, TInput, Task<TOutput>> next)
        {
            var customAuthorizers = AuthorizeWhenAttribute.GetAuthorizers(input);
            var user = appContext.User;

            if (customAuthorizers.Count > 0)
            {
                if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    throw new UnauthenticatedException(
                        "The current user is not authenticated and the object requires authorization");
                }
            }

            var objectType = input.GetType();
            foreach (var customAuthorizerDefinition in customAuthorizers)
            {
                var authorizerType = customAuthorizerDefinition.Authorizer;
                var customAuthorizer = authorizerResolver.FindAuthorizer(
                    authorizerType, objectType);

                if (customAuthorizer == null)
                {
                    throw new CustomAuthorizerNotFoundException(authorizerType);
                }

                var authorized = await customAuthorizer
                    .CheckIfAuthorizedAsync(appContext, input, customAuthorizerDefinition.CustomData)
                    .ConfigureAwait(false);
                if (!authorized)
                {
                    logger.Warning("Authorizer {Authorizer} failed to authorize the user to run {@Object}",
                        customAuthorizer.UnderlyingAuthorizer.FullName, input);
                    throw new InsufficientPermissionException(
                        $"User is not authorized for {input.GetType()}.");
                }
            }

            return await next(appContext, input).ConfigureAwait(false);
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, CommandExecutionPayload, CommandResult> Secure<TContext>(
            this PipelineBuilder<TContext, CommandExecutionPayload, CommandResult> builder)
            where TContext : ISecurityContext
        {
            return builder.Use<CQRSSecurityElement<TContext, CommandExecutionPayload, CommandResult>>();
        }

        public static PipelineBuilder<TContext, QueryExecutionPayload, object> Secure<TContext>(
            this PipelineBuilder<TContext, QueryExecutionPayload, object> builder)
            where TContext : ISecurityContext
        {
            return builder.Use<CQRSSecurityElement<TContext, QueryExecutionPayload, object>>();
        }
    }
}
