using System;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Autofac
{
    class AutofacAuthorizerResolver<TAppContext> : IAuthorizerResolver<TAppContext>
    {
        private static readonly Type AuthorizerBase = typeof(ICustomAuthorizer<TAppContext>);
        private static readonly Type QueryAuthorizerWrapperBase = typeof(CustomQueryAuthorizerWrapper<TAppContext>);
        private static readonly Type CommandAuthorizerWrapperBase = typeof(CustomCommandAuthorizerWrapper<TAppContext>);
        private static readonly TypesCache typesCache = new TypesCache(
            a => AuthorizerBase.MakeGenericType(a),
            ConstructWrapperType);

        private readonly IComponentContext componentContext;

        public AutofacAuthorizerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICustomAuthorizerWrapper FindAuthorizer(Type authorizerType, Type objectType)
        {
            var cached = typesCache.Get(objectType);
            if (componentContext.TryResolve(authorizerType, out var handler))
            {
                var wrapper = cached.Constructor.Invoke(new[] { handler });
                return (ICustomAuthorizerWrapper)wrapper;
            }
            else
            {
                return null;
            }
        }

        private static Type ConstructWrapperType(Type objType)
        {
            if (typeof(IQuery).IsAssignableFrom(objType))
            {
                return QueryAuthorizerWrapperBase;
            }
            else
            {
                return CommandAuthorizerWrapperBase;
            }
        }
    }
}
