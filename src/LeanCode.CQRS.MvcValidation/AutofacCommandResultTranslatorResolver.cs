using Autofac;

namespace LeanCode.CQRS.MvcValidation
{
    class AutofacCommandResultTranslatorResolver : ICommandResultTranslatorResolver
    {
        private readonly IComponentContext componentContext;

        public AutofacCommandResultTranslatorResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandResultTranslator<TCommand> Resolve<TCommand>()
            where TCommand : ICommand
        {
            ICommandResultTranslator<TCommand> validator;
            componentContext.TryResolve<ICommandResultTranslator<TCommand>>(out validator);
            return validator;
        }
    }
}
