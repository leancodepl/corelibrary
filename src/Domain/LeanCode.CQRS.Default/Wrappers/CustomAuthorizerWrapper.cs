using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Wrappers
{
    class CustomQueryAuthorizerWrapper<TAppContext> : ICustomAuthorizerWrapper
    {
        private readonly ICustomAuthorizer<TAppContext> authorizer;

        public Type UnderlyingAuthorizer { get; }

        public CustomQueryAuthorizerWrapper(ICustomAuthorizer<TAppContext> authorizer)
        {
            this.authorizer = authorizer;
            UnderlyingAuthorizer = authorizer.GetType();
        }

        public Task<bool> CheckIfAuthorizedAsync(object appContext, object obj, object customData)
        {
            return authorizer.CheckIfAuthorizedAsync((TAppContext)appContext, (QueryExecutionPayload)obj, customData);
        }
    }

    class CustomCommandAuthorizerWrapper<TAppContext> : ICustomAuthorizerWrapper
    {
        private readonly ICustomAuthorizer<TAppContext> authorizer;

        public Type UnderlyingAuthorizer { get; }

        public CustomCommandAuthorizerWrapper(ICustomAuthorizer<TAppContext> authorizer)
        {
            this.authorizer = authorizer;
            UnderlyingAuthorizer = authorizer.GetType();
        }

        public Task<bool> CheckIfAuthorizedAsync(object appContext, object obj, object customData)
        {
            return authorizer.CheckIfAuthorizedAsync((TAppContext)appContext, (CommandExecutionPayload)obj, customData);
        }
    }
}
