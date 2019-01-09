using System;
using System.Threading.Tasks;

namespace LeanCode.CQRS.Security
{
    public interface ICustomAuthorizer<in TAppContext>
    {
        Task<bool> CheckIfAuthorizedAsync(TAppContext appContext, object obj, object customData);
    }

    public abstract class CustomAuthorizer<TAppContext, TObject>
        : ICustomAuthorizer<TAppContext>
    {
        public Task<bool> CheckIfAuthorizedAsync(
            TAppContext appContext,
            object obj,
            object customData)
        {
            return CheckIfAuthorizedAsync(appContext, (TObject)obj);
        }

        protected abstract Task<bool> CheckIfAuthorizedAsync(
            TAppContext appContext, TObject obj);
    }

    public abstract class CustomAuthorizer<TAppContext, TObject, TCustomData>
        : ICustomAuthorizer<TAppContext>
        where TCustomData : class
    {
        public Task<bool> CheckIfAuthorizedAsync(TAppContext appContext, object obj, object customData)
        {
            return CheckIfAuthorizedInternal(appContext, (TObject)obj, customData);
        }

        protected abstract Task<bool> CheckIfAuthorizedAsync(
            TAppContext appContext, TObject obj, TCustomData customData);

        private Task<bool> CheckIfAuthorizedInternal(TAppContext appContext, TObject obj, object customData)
        {
            if (!(customData is null) && !(customData is TCustomData))
            {
                throw new ArgumentException(
                    $"{GetType()} requires {typeof(TCustomData)} as custom data.",
                    nameof(customData));
            }

            return CheckIfAuthorizedAsync(appContext, obj, (TCustomData)customData);
        }
    }
}
