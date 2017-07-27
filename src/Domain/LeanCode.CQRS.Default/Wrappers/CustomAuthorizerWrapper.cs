using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;

namespace LeanCode.CQRS.Default.Wrappers
{
    class CustomAuthorizerWrapper<TContext, TObject> : ICustomAuthorizerWrapper
    {
        private readonly ICustomAuthorizer<TContext, TObject> authorizer;

        public Type UnderlyingAuthorizer { get; }

        public CustomAuthorizerWrapper(ICustomAuthorizer<TContext, TObject> authorizer)
        {
            this.authorizer = authorizer;
            UnderlyingAuthorizer = authorizer.GetType();
        }

        public Task<bool> CheckIfAuthorized(object context, object obj, object customData)
        {
            return authorizer.CheckIfAuthorized((TContext)context, (TObject)obj, customData);
        }
    }
}
