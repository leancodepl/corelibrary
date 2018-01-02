using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Validation
{
    public class ValidationElement<TAppContext>
        : IPipelineElement<TAppContext, CommandExecutionPayload, CommandResult>
        where TAppContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ValidationElement<TAppContext>>();

        private readonly ICommandValidatorResolver<TAppContext> resolver;

        public ValidationElement(ICommandValidatorResolver<TAppContext> resolver)
        {
            this.resolver = resolver;
        }

        public async Task<CommandResult> ExecuteAsync(
            TAppContext appContext,
            CommandExecutionPayload payload,
            Func<TAppContext, CommandExecutionPayload, Task<CommandResult>> next)
        {
            var context = payload.Context;
            var command = payload.Command;

            var commandType = command.GetType();
            var validator = resolver.FindCommandValidator(commandType);
            if (validator != null)
            {
                var result = await validator
                    .ValidateAsync(appContext, context, command)
                    .ConfigureAwait(false);
                if (!result.IsValid)
                {
                    logger.Information("Command {@Command} is not valid with result {@Result}",
                        command, result);
                    return CommandResult.NotValid(result);
                }
            }
            return await next(appContext, payload).ConfigureAwait(false);
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TAppContext, CommandExecutionPayload, CommandResult> Validate<TAppContext>(
            this PipelineBuilder<TAppContext, CommandExecutionPayload, CommandResult> builder)
            where TAppContext : IPipelineContext
        {
            return builder.Use<ValidationElement<TAppContext>>();
        }
    }
}
