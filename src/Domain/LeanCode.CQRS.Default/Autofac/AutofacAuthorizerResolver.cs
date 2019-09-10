using System;
using Autofac;
using LeanCode.CQRS.Default.Wrappers;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Autofac
{
    internal class AutofacAuthorizerResolver<TAppContext> : IAuthorizerResolver<TAppContext>
        where TAppContext : notnull
    {
        private readonly IComponentContext componentContext;

        public AutofacAuthorizerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICustomAuthorizerWrapper? FindAuthorizer(Type authorizerType, Type objectType)
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
