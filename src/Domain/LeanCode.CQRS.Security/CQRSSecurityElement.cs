using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Security.Exceptions;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Security
{
    public class CQRSSecurityElement<TAppContext, TInput, TOutput> : IPipelineElement<TAppContext, TInput, TOutput>
        where TAppContext : ISecurityContext
        where TInput : notnull
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
            var objectType = input.GetType();
            var customAuthorizers = AuthorizeWhenAttribute.GetCustomAuthorizers(objectType);
            var user = appContext.User;

            if (customAuthorizers.Count > 0 && !(user?.Identity?.IsAuthenticated ?? false))
            {
                logger.Warning("Unauthenticated user requested object that requires authorization");

                throw new UnauthenticatedException(
                    "The current user is not authenticated and the object requires authorization");
            }

            foreach (var customAuthorizerDefinition in customAuthorizers)
            {
                var authorizerType = customAuthorizerDefinition.Authorizer;
                var customAuthorizer = authorizerResolver.FindAuthorizer(authorizerType, objectType);

                if (customAuthorizer is null)
                {
                    throw new CustomAuthorizerNotFoundException(authorizerType);
                }

                var authorized = await customAuthorizer
                    .CheckIfAuthorizedAsync(appContext, input, customAuthorizerDefinition.CustomData);

                if (!authorized)
                {
                    logger.Warning(
                        "Authorizer {Authorizer} failed to authorize the user to run {@Object}",
                        customAuthorizer.UnderlyingAuthorizer.FullName, input);

                    throw new InsufficientPermissionException(
                        $"User is not authorized for {input.GetType()}.");
                }
            }

            return await next(appContext, input);
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, ICommand, CommandResult> Secure<TContext>(
            this PipelineBuilder<TContext, ICommand, CommandResult> builder)
            where TContext : ISecurityContext
        {
            return builder.Use<CQRSSecurityElement<TContext, ICommand, CommandResult>>();
        }

        public static PipelineBuilder<TContext, IQuery, object?> Secure<TContext>(
            this PipelineBuilder<TContext, IQuery, object?> builder)
            where TContext : ISecurityContext
        {
            return builder.Use<CQRSSecurityElement<TContext, IQuery, object?>>();
        }
    }
}
