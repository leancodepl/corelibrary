using System;
using System.Collections.Concurrent;
using System.Reflection;
using Autofac;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Default
{
    class AutofacValidatorResolver : ICommandValidatorResolver
    {
        private static readonly Type ValidatorBase = typeof(ICommandValidator<>);
        private static readonly Type ValidatorWrapperBase = typeof(CommandValidatorWrapper<>);

        private static readonly ConcurrentDictionary<Type, Tuple<Type, ConstructorInfo>> typesCache =
            new ConcurrentDictionary<Type, Tuple<Type, ConstructorInfo>>();

        private readonly IComponentContext componentContext;

        public AutofacValidatorResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICommandValidatorWrapper FindCommandValidator(Type commandType)
        {
            var cached = typesCache.GetOrAdd(commandType, _ =>
            {
                var queryHandlerType = ValidatorBase.MakeGenericType(commandType);
                var wrappedHandlerType = ValidatorWrapperBase.MakeGenericType(commandType);
                var ctor = wrappedHandlerType.GetConstructors()[0];
                return Tuple.Create(queryHandlerType, ctor);
            });

            componentContext.TryResolve(cached.Item1, out var handler);

            if (handler == null)
            {
                return null;
            }
            return (ICommandValidatorWrapper)cached.Item2.Invoke(new[] { handler });
        }
    }
}
