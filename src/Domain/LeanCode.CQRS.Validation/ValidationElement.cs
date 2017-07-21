using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Validation
{
    public class ValidationElement
        : IPipelineElement<ExecutionContext, ICommand, CommandResult>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ValidationElement>();

        private readonly ICommandValidatorResolver resolver;

        public ValidationElement(ICommandValidatorResolver resolver)
        {
            this.resolver = resolver;
        }

        public async Task<CommandResult> ExecuteAsync(
            ExecutionContext ctx,
            ICommand input,
            Func<ExecutionContext, ICommand, Task<CommandResult>> next)
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
            return await next(ctx, input).ConfigureAwait(false);
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<ExecutionContext, ICommand, CommandResult> Validate(
            this PipelineBuilder<ExecutionContext, ICommand, CommandResult> builder
        )
        {
            return builder.Use<ValidationElement>();
        }
    }
}
