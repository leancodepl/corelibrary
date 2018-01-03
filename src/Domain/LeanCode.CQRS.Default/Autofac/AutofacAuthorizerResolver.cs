using System;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Autofac
{
    class AutofacAuthorizerResolver<TAppContext> : IAuthorizerResolver<TAppContext>
    {
        private readonly IComponentContext componentContext;

        public AutofacAuthorizerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICustomAuthorizerWrapper FindAuthorizer(Type authorizerType, Type objectType)
        {
            if (componentContext.TryResolve(authorizerType, out var handler))
            {
                var typed = (ICustomAuthorizer<TAppContext>)handler;
                return new CustomAuthorizerWrapper<TAppContext>(typed);
            }
            else
            {
                return null;
            }
        }
    }
}
