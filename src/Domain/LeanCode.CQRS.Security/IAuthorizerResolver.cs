using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Security
{
    public interface IAuthorizerResolver
    {
        ICustomAuthorizerWrapper FindAuthorizer(Type contextType, Type authorizerType, Type objectType);
    }

    public interface ICustomAuthorizerWrapper
    {
        Type UnderlyingAuthorizer { get; }
        Task<bool> CheckIfAuthorized(object context, object obj, object customData);
    }
}
