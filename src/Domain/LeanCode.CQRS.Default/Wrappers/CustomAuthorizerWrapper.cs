using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Wrappers
{
    class CustomAuthorizerWrapper<TAppContext> : ICustomAuthorizerWrapper
    {
        private readonly ICustomAuthorizer<TAppContext> authorizer;

        public Type UnderlyingAuthorizer { get; }

        public CustomAuthorizerWrapper(ICustomAuthorizer<TAppContext> authorizer)
        {
            this.authorizer = authorizer;
            UnderlyingAuthorizer = authorizer.GetType();
        }

        public Task<bool> CheckIfAuthorizedAsync(object appContext, object obj, object customData)
        {
            return authorizer.CheckIfAuthorizedAsync((TAppContext)appContext, obj, customData);
        }
    }
}
