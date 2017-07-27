using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Validation
{
    public class ValidationElement<TContext>
        : IPipelineElement<TContext, ICommand, CommandResult>
        where TContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ValidationElement<TContext>>();

        private readonly ICommandValidatorResolver resolver;

        public ValidationElement(ICommandValidatorResolver resolver)
        {
            this.resolver = resolver;
        }

        public async Task<CommandResult> ExecuteAsync(
            TContext context,
            ICommand input,
            Func<TContext, ICommand, Task<CommandResult>> next)
        {
            var contextType = context.GetType();
            var commandType = input.GetType();
            var validator = resolver.FindCommandValidator(contextType, commandType);
            if (validator != null)
            {
                var result = await validator
                    .ValidateAsync(context, input)
                    .ConfigureAwait(false);
                if (!result.IsValid)
                {
                    logger.Information("Command {@Command} is not valid", input);
                    return CommandResult.NotValid(result);
                }
            }
            return await next(context, input).ConfigureAwait(false);
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TContext, ICommand, CommandResult> Validate<TContext>(
            this PipelineBuilder<TContext, ICommand, CommandResult> builder)
            where TContext : IPipelineContext
        {
            return builder.Use<ValidationElement<TContext>>();
        }
    }
}
