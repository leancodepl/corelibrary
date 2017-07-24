using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Security
{
    public interface ICustomAuthorizer
    {
        Task<bool> CheckIfAuthorized(ClaimsPrincipal user, object obj, object customData = null);
    }

    public abstract class CustomAuthorizer<TObject, TCustomData> : ICustomAuthorizer
        where TObject : class
        where TCustomData : class
    {
        public Task<bool> CheckIfAuthorized(ClaimsPrincipal user, object obj, object customData = null)
        {
            var customAuthorizer = obj as TObject;
            if (customAuthorizer == null)
            {
                throw new ArgumentException(
                    $"{GetType()} is not valid Authorizer for {obj.GetType()}.",
                    nameof(customAuthorizer));
            }
            if (customData != null && !(customData is TCustomData))
            {
                throw new ArgumentException(
                    $"{GetType()} requires {typeof(TCustomData)} as custom data.",
                    nameof(customData));
            }

            return CheckIfAuthorized(user, customAuthorizer,
                (TCustomData)customData);
        }

        public abstract Task<bool> CheckIfAuthorized(
            ClaimsPrincipal user,
            TObject obj, TCustomData customData = null);
    }

    public abstract class CustomAuthorizer<TObject> : CustomAuthorizer<TObject, object>
        where TObject : class
    {
        public override Task<bool> CheckIfAuthorized(
            ClaimsPrincipal user,
            TObject obj, object customData = null)
        {
            return CheckIfAuthorized(user, obj);
        }

        public abstract Task<bool> CheckIfAuthorized(
            ClaimsPrincipal user, TObject obj);
    }
}
