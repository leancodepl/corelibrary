using System;
using Autofac;
using LeanCode.CQRS.Security;

namespace LeanCode.Domain.Default
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
            componentContext.TryResolve(type, out var authorizer);
            return authorizer as ICustomAuthorizer;
        }
    }
}
