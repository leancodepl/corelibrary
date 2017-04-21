using System;
using Autofac;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Autofac.Security
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
            object authorizer;
            componentContext.TryResolve(type, out authorizer);
            return authorizer as ICustomAuthorizer;
        }
    }
}
