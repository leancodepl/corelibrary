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
            TObject customAuthorizer = obj as TObject;
            if (customAuthorizer == null)
            {
                throw new ArgumentNullException(nameof(customAuthorizer),
                    $"{GetType()} is not valid Authorizer for {obj.GetType()}.");
            }

            return CheckIfAuthorized(customAuthorizer, customData as TCustomData);
        }

        public abstract Task<bool> CheckIfAuthorized(TObject obj, TCustomData customData = null);
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
