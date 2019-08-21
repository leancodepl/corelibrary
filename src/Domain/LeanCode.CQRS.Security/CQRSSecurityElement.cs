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
        where TInput : IExecutionPayload
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
            var objectType = input.Object.GetType();
            var customAuthorizers = AuthorizeWhenAttribute.GetCustomAuthorizers(objectType);
            var user = appContext.User;

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
                var authorizerType = customAuthorizerDefinition.Authorizer;
                var customAuthorizer = authorizerResolver.FindAuthorizer(
                    authorizerType, objectType);

                if (customAuthorizer == null)
                {
                    throw new CustomAuthorizerNotFoundException(authorizerType);
                }

                var authorized = await customAuthorizer
                    .CheckIfAuthorizedAsync(appContext, input.Context, input.Object, customAuthorizerDefinition.CustomData)
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
        /// <summary>
        /// Registers <see cref="CQRSSecurityElement{TAppContext, TInput, TOutput}" /> pipeline element
        /// which executes authorizers for commands attributed with <see cref="AuthorizeWhenAttribute"/>
        /// </summary>
        public static PipelineBuilder<TContext, CommandExecutionPayload, CommandResult> Secure<TContext>(
            this PipelineBuilder<TContext, CommandExecutionPayload, CommandResult> builder)
            where TContext : ISecurityContext
        {
            return builder.Use<CQRSSecurityElement<TContext, CommandExecutionPayload, CommandResult>>();
        }

        /// <summary>
        /// Registers <see cref="CQRSSecurityElement{TAppContext, TInput, TOutput}" /> pipeline element
        /// which executes authorizers for queries attributed with <see cref="AuthorizeWhenAttribute"/>
        /// </summary>
        public static PipelineBuilder<TContext, QueryExecutionPayload, object> Secure<TContext>(
            this PipelineBuilder<TContext, QueryExecutionPayload, object> builder)
            where TContext : ISecurityContext
        {
            return builder.Use<CQRSSecurityElement<TContext, QueryExecutionPayload, object>>();
        }
    }
}
