using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Threading.Tasks;

namespace LeanCode.CQRS.MvcValidation
{
    public sealed class ValidatedCommandExecutor
    {
        private readonly ICommandExecutor commandExecutor;
        private readonly ICommandResultTranslatorResolver resolver;
        private readonly IMapper mapper;
        private readonly IActionContextAccessor contextAccesor;

        public ValidatedCommandExecutor(
            ICommandExecutor commandExecutor,
            ICommandResultTranslatorResolver resolver,
            IMapper mapper,
            IActionContextAccessor contextAccesor)
        {
            this.commandExecutor = commandExecutor;
            this.resolver = resolver;
            this.mapper = mapper;
            this.contextAccesor = contextAccesor;
        }

        public Task<bool> ValidateAndExecuteAsync<TCommand>(object vm)
            where TCommand : ICommand
        {
            var modelState = contextAccesor.ActionContext.ModelState;
            if (modelState.IsValid)
            {
                var cmd = mapper.Map<TCommand>(vm);
                return Execute(cmd, modelState);
            }
            return Task.FromResult(false);
        }

        public Task<bool> ValidateAndExecuteAsync<TCommand>(object vm, Action<IMappingOperationOptions> opts)
            where TCommand : ICommand
        {
            var modelState = contextAccesor.ActionContext.ModelState;
            if (modelState.IsValid)
            {
                var cmd = mapper.Map<TCommand>(vm, opts);
                return Execute(cmd, modelState);
            }
            return Task.FromResult(false);
        }

        public Task<bool> ExecuteAsync<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            var modelState = contextAccesor.ActionContext.ModelState;
            return Execute(command, modelState);
        }

        private Task<bool> Execute<TCommand>(TCommand command, ModelStateDictionary modelState)
            where TCommand : ICommand
        {
            var translator = resolver.Resolve<TCommand>();
            if (translator == null)
            {
                return ExecuteAndMap(command, modelState);
            }
            else
            {
                return ExecuteAndTranslate(command, translator, modelState);
            }
        }

        private async Task<bool> ExecuteAndMap<TCommand>(TCommand command, ModelStateDictionary modelState)
            where TCommand : ICommand
        {
            var result = await commandExecutor.ExecuteAsync(command).ConfigureAwait(false);
            if (!result.WasSuccessful)
            {
                DirectMapToModelState(result, modelState);
            }
            return result.WasSuccessful;
        }

        private async Task<bool> ExecuteAndTranslate<TCommand>(
            TCommand command,
            ICommandResultTranslator<TCommand> translator,
            ModelStateDictionary modelState)
            where TCommand : ICommand
        {
            try
            {
                var result = await commandExecutor.ExecuteAsync(command).ConfigureAwait(false);
                if (!result.WasSuccessful)
                {
                    MapResultToModelState(command, result, translator, modelState);
                }
                return result.WasSuccessful;
            }
            catch (Exception ex) when (translator.CanHandle(command, ex))
            {
                string translated = translator.Translate(command, ex);
                modelState.AddModelError("", translated);
                return false;
            }
        }

        private void MapResultToModelState<TCommand>(
            TCommand command,
            CommandResult result,
            ICommandResultTranslator<TCommand> translator,
            ModelStateDictionary modelState)
            where TCommand : ICommand
        {
            foreach (var err in result.ValidationErrors)
            {
                var property = translator.TranslateProperty(err.PropertyName) ?? err.PropertyName;
                var message = translator.Translate(command, err.ErrorCode) ?? err.ErrorMessage;
                modelState.AddModelError(property, message);
            }
        }

        private void DirectMapToModelState(CommandResult result, ModelStateDictionary modelState)
        {
            foreach (var err in result.ValidationErrors)
            {
                modelState.AddModelError(err.PropertyName, err.ErrorMessage);
            }
        }
    }
}
