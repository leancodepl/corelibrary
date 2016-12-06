using System;
using Autofac;

namespace LeanCode.CQRS.MvcValidation
{
    public class AutofacCommandResultTranslatorResolver : ICommandResultTranslatorResolver
    {
        private readonly Func<IComponentContext> componentContext;

        public AutofacCommandResultTranslatorResolver(Func<IComponentContext> componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandResultTranslator<TCommand> Resolve<TCommand>()
            where TCommand : ICommand
        {
            ICommandResultTranslator<TCommand> validator;
            componentContext().TryResolve<ICommandResultTranslator<TCommand>>(out validator);
            return validator;
        }
    }
}
