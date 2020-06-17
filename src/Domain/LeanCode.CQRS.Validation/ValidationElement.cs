using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Validation
{
    public class ValidationElement<TAppContext> : IPipelineElement<TAppContext, ICommand, CommandResult>
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
            ICommand payload,
            Func<TAppContext, ICommand, Task<CommandResult>> next)
        {
            var commandType = payload.GetType();
            var validator = resolver.FindCommandValidator(commandType);

            if (validator is null)
            {
                return await next(appContext, payload);
            }

            var result = await validator.ValidateAsync(appContext, payload);

            if (!result.IsValid)
            {
                logger.Warning("Command {@Command} is not valid with result {@Result}", payload, result);

                return CommandResult.NotValid(result);
            }

            return await next(appContext, payload);
        }
    }

    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TAppContext, ICommand, CommandResult> Validate<TAppContext>(
            this PipelineBuilder<TAppContext, ICommand, CommandResult> builder)
            where TAppContext : IPipelineContext
        {
            return builder.Use<ValidationElement<TAppContext>>();
        }
    }
}
