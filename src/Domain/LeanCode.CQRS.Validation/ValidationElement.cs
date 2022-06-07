using System;
using System.Threading.Tasks;
using LeanCode.Contracts;
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
            TAppContext ctx,
            ICommand input,
            Func<TAppContext, ICommand, Task<CommandResult>> next)
        {
            var commandType = input.GetType();
            var validator = resolver.FindCommandValidator(commandType);

            if (validator is null)
            {
                return await next(ctx, input);
            }

            var result = await validator.ValidateAsync(ctx, input);

            if (!result.IsValid)
            {
                logger.Warning("Command {@Command} is not valid with result {@Result}", input, result);

                return CommandResult.NotValid(result);
            }

            return await next(ctx, input);
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
