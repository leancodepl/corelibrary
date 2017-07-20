using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Security
{
    public interface ICustomAuthorizer
    {
        Task<bool> CheckIfAuthorized(object obj, object customData = null);
    }

    public abstract class CustomAuthorizer<TObject, TCustomData> : ICustomAuthorizer
        where TObject : class
        where TCustomData : class
    {
        public Task<bool> CheckIfAuthorized(object obj, object customData = null)
        {
            var customAuthorizer = obj as TObject;
            if (customAuthorizer == null)
            {
                throw new ArgumentException(
                    $"{GetType()} is not valid Authorizer for {obj.GetType()}.",
                    nameof(customAuthorizer));
            }
            var data = customData as TCustomData;
            if (data == null)
            {
                throw new ArgumentException(
                    $"{GetType()} requires {typeof(TCustomData)} as custom data.",
                    nameof(customData));
            }

            return CheckIfAuthorized(customAuthorizer, data);
        }

        public abstract Task<bool> CheckIfAuthorized(
            TObject obj, TCustomData customData = null);
    }

    public abstract class CustomAuthorizer<TObject> : CustomAuthorizer<TObject, object>
        where TObject : class
    {
        public override Task<bool> CheckIfAuthorized(TObject obj,
            object customData = null)
        {
            return CheckIfAuthorized(obj);
        }

        public abstract Task<bool> CheckIfAuthorized(TObject obj);
    }
}
