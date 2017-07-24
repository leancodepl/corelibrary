using System;
using Autofac;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Autofac
{
    class AutofacAuthorizerResolver : IAuthorizerResolver
    {
        private readonly IComponentContext componentContext;

        public AutofacAuthorizerResolver(IComponentContext componentContext)
        {
            this.componentContext = componentContext;
        }

        public ICustomAuthorizer FindAuthorizer(Type type)
        {
            return (ICustomAuthorizer)componentContext.Resolve(type);
        }
    }
}
