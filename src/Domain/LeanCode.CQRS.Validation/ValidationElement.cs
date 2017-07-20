using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Validation
{
    public class ValidationElement : IPipelineElement<ICommand, CommandResult>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ValidationElement>();

        private readonly ICommandValidatorResolver resolver;

        public ValidationElement(ICommandValidatorResolver resolver)
        {
            this.resolver = resolver;
        }

        public async Task<CommandResult> ExecuteAsync(
            ICommand input,
            Func<ICommand, Task<CommandResult>> next)
        {
            var commandType = input.GetType();
            var validator = resolver.FindCommandValidator(commandType);
            if (validator != null)
            {
                var result = await validator
                    .ValidateAsync(input)
                    .ConfigureAwait(false);
                if (!result.IsValid)
                {
                    logger.Information("Command {@Command} is not valid", input);
                    return CommandResult.NotValid(result);
                }
            }
            return await next(input).ConfigureAwait(false);
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<ICommand, CommandResult> Validate(
            this PipelineBuilder<ICommand, CommandResult> builder
        )
        {
            return builder.Use<ValidationElement>();
        }
    }
}
