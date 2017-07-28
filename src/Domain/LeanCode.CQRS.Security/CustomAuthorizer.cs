using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Security
{
    public interface ICustomAuthorizer<in TAppContext, in TObject>
    {
        Task<bool> CheckIfAuthorized(TAppContext context, TObject obj, object customData);
    }

    public abstract class CustomAuthorizer<TAppContext, TObject> : ICustomAuthorizer<TAppContext, TObject>
    {
        public Task<bool> CheckIfAuthorized(
            TAppContext context, TObject obj, object customData)
        {
            return CheckIfAuthorized(context, obj);
        }

        protected abstract Task<bool> CheckIfAuthorized(
            TAppContext context, TObject obj);
    }

    public abstract class CustomAuthorizer<TAppContext, TObject, TCustomData>
        : ICustomAuthorizer<TAppContext, TObject>
        where TCustomData : class
    {
        public Task<bool> CheckIfAuthorized(TAppContext context, TObject obj, object customData)
        {
            if (customData != null && !(customData is TCustomData))
            {
                throw new ArgumentException(
                    $"{GetType()} requires {typeof(TCustomData)} as custom data.",
                    nameof(customData));
            }

            return CheckIfAuthorized(context, obj, (TCustomData)customData);
        }

        protected abstract Task<bool> CheckIfAuthorized(
            TAppContext context, TObject obj, TCustomData customData);
    }

}
