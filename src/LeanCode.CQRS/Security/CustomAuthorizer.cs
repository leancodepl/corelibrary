using System;

namespace LeanCode.CQRS.Security
{
    public interface ICustomAuthorizer
    {
        bool CheckIfAuthorized(object obj, object customData = null);
    }

    public abstract class CustomAuthorizer<TObject, TCustomData> : ICustomAuthorizer
        where TObject : class
        where TCustomData : class
    {
        public bool CheckIfAuthorized(object obj, object customData = null)
        {
            TObject customAuthorizer = obj as TObject;
            if (customAuthorizer == null)
            {
                throw new ArgumentNullException(nameof(customAuthorizer), $"{GetType()} is not valid Authorizer for {obj.GetType()}.");
            }

            return CheckIfAuthorized(customAuthorizer, customData as TCustomData);
        }

        public abstract bool CheckIfAuthorized(TObject obj, TCustomData customData = null);
    }

    public abstract class CustomAuthorizer<TObject> : CustomAuthorizer<TObject, object>
        where TObject : class
    { }
}
