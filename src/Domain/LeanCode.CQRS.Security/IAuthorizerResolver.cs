using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Security
{
    public interface IAuthorizerResolver<TAppContext>
    {
        ICustomAuthorizerWrapper FindAuthorizer(Type authorizerType, Type objectType);
    }

    public interface ICustomAuthorizerWrapper
    {
        Type UnderlyingAuthorizer { get; }
        Task<bool> CheckIfAuthorizedAsync(object appContext, object obj, object customData);
    }
}
