using System;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Autofac
{
    class AutofacAuthorizerResolver : IAuthorizerResolver
    {
        private static readonly Type AuthorizerBase = typeof(ICustomAuthorizer<,>);
        private static readonly Type AuthorizerWrapperBase = typeof(CustomAuthorizerWrapper<,>);
        private static readonly TypesCache typesCache = new TypesCache(AuthorizerBase, AuthorizerWrapperBase);

        private readonly IComponentContext componentContext;

        public AutofacAuthorizerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICustomAuthorizerWrapper FindAuthorizer(Type contextType, Type authorizerType, Type objectType)
        {
            var cached = typesCache.Get(contextType, objectType);
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
    }
}
