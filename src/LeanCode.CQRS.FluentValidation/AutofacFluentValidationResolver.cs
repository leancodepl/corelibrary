using Autofac;
using FluentValidation;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.FluentValidation
{
    class AutofacFluentValidatorResolver : ICommandValidatorResolver
    {
        private readonly IComponentContext componentContext;

        public AutofacFluentValidatorResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandValidator<TCommand> GetValidator<TCommand>()
            where TCommand : ICommand
        {
            IValidator<TCommand> validator;
            if (componentContext.TryResolve<IValidator<TCommand>>(out validator))
            {
                return new FluentValidationCommandValidatorAdapter<TCommand>(validator, this.componentContext);
            }
            return null;
        }
    }
}
