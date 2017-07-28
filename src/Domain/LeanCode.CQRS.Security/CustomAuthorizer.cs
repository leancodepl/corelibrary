using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Security
{
    public interface ICustomAuthorizer<in TAppContext, in TObject>
    {
        Task<bool> CheckIfAuthorized(TAppContext context, TObject obj, object customData = null);
    }

    public abstract class CustomAuthorizer<TAppContext, TObject, TCustomData>
        : ICustomAuthorizer<TAppContext, TObject>
        where TCustomData : class
    {
        public Task<bool> CheckIfAuthorized(TAppContext context, TObject obj, object customData = null)
        {
            if (customData != null && !(customData is TCustomData))
            {
                throw new ArgumentException(
                    $"{GetType()} requires {typeof(TCustomData)} as custom data.",
                    nameof(customData));
            }

            return RealCheckIfAuthorized(context, obj, (TCustomData)customData);
        }

        protected abstract Task<bool> RealCheckIfAuthorized(
            TAppContext context,
            TObject obj, TCustomData customData = null);
    }

    public abstract class CustomAuthorizer<TAppContext, TObject> : CustomAuthorizer<TAppContext, TObject, object>
        where TObject : class
    {
        protected override Task<bool> RealCheckIfAuthorized(
            TAppContext context,
            TObject obj, object customData = null)
        {
            return CheckIfAuthorized(context, obj);
        }

        protected abstract Task<bool> RealCheckIfAuthorized(
            TAppContext context, TObject obj);
    }
}
