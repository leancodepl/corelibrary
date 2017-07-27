using System;
using System.Collections.Concurrent;
using System.Reflection;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Default.Autofac
{
    class AutofacValidatorResolver : ICommandValidatorResolver
    {
        private static readonly Type ValidatorBase = typeof(ICommandValidator<,>);
        private static readonly Type WrapperBase = typeof(CommandValidatorWrapper<,>);

        private static readonly TypesCache typesCache = new TypesCache(ValidatorBase, WrapperBase);

        private readonly IComponentContext componentContext;

        public AutofacValidatorResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandValidatorWrapper FindCommandValidator(
            Type contextType, Type commandType)
        {
            var cached = typesCache.Get(contextType, commandType);

            if (componentContext.TryResolve(cached.HandlerType, out var handler))
            {
                var cv = cached.Constructor.Invoke(new[] { handler });
                return (ICommandValidatorWrapper)cv;
            }
            else
            {
                return null;
            }
        }
    }
}
