using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.Pipelines;

namespace LeanCode.ExternalIdentityProviders
{
    public class ExternalLoginExceptionHandler<TContext> : IPipelineElement<TContext, ICommand, CommandResult>
        where TContext : notnull, IPipelineContext
    {
        public const int ErrorCodeInvalidToken = 10000;
        public const int ErrorCodeOtherConnected = 10001;
        public const int ErrorCodeOther = 10002;

        public async Task<CommandResult> ExecuteAsync(
            TContext ctx,
            ICommand input,
            Func<TContext, ICommand, Task<CommandResult>> next)
        {
            try
            {
                return await next(ctx, input);
            }
            catch (ExternalLoginException ex)
            {
                ValidationError err = ex.TokenValidation switch
                {
                    TokenValidationError.Invalid => new(
                        "",
                        "The token is invalid.",
                        ErrorCodeInvalidToken),

                    TokenValidationError.OtherConnected => new(
                        "",
                        "Other account is already connected with this token.",
                        ErrorCodeOtherConnected),

                    _ => new(
                        "",
                        "Cannot perform external login.",
                        ErrorCodeOther),
                };

                return new(ImmutableList.Create(err));
            }
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, ICommand, CommandResult> HandleExternalLoginExceptions<TContext>(
            this PipelineBuilder<TContext, ICommand, CommandResult> builder)
            where TContext : IPipelineContext
        {
            return builder.Use<ExternalLoginExceptionHandler<TContext>>();
        }
    }
}
