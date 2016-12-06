using System;
using Autofac;
using FluentValidation;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.FluentValidation
{
    class AutofacFluentValidatorResolver : ICommandValidatorResolver
    {
        private readonly Func<IComponentContext> componentContext;

        public AutofacFluentValidatorResolver(Func<IComponentContext> componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandValidator<TCommand> GetValidator<TCommand>()
            where TCommand : ICommand
        {
            IValidator<TCommand> validator;
            if (componentContext().TryResolve<IValidator<TCommand>>(out validator))
            {
                return new FluentValidationCommandValidatorAdapter<TCommand>(validator);
            }
            return null;
        }
    }
}
