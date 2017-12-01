using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Security
{
    public interface ICustomAuthorizer<TAppContext>
    {
        Task<bool> CheckIfAuthorizedAsync(TAppContext appContext, QueryExecutionPayload payload, object customData);
        Task<bool> CheckIfAuthorizedAsync(TAppContext appContext, CommandExecutionPayload payload, object customData);
    }

    public abstract class CustomAuthorizer<TAppContext, TContext, TObject>
        : ICustomAuthorizer<TAppContext>
    {
        public Task<bool> CheckIfAuthorizedAsync(
            TAppContext appContext,
            QueryExecutionPayload payload,
            object customData)
        {
            return CheckIfAuthorizedAsync(appContext, (TContext)payload.Context, (TObject)payload.Query);
        }

        public Task<bool> CheckIfAuthorizedAsync(
            TAppContext appContext,
            CommandExecutionPayload payload,
            object customData)
        {
            return CheckIfAuthorizedAsync(appContext, (TContext)payload.Context, (TObject)payload.Command);
        }

        protected abstract Task<bool> CheckIfAuthorizedAsync(
            TAppContext appContext, TContext objContext, TObject obj);
    }

    public abstract class CustomAuthorizer<TAppContext, TContext, TObject, TCustomData>
        : ICustomAuthorizer<TAppContext>
        where TCustomData : class
    {
        public Task<bool> CheckIfAuthorizedAsync(TAppContext appContext, QueryExecutionPayload payload, object customData)
        {
            return CheckIfAuthorizedInternal(appContext, (TContext)payload.Context, (TObject)payload.Query, customData);
        }

        public Task<bool> CheckIfAuthorizedAsync(TAppContext appContext, CommandExecutionPayload payload, object customData)
        {
            return CheckIfAuthorizedInternal(appContext, (TContext)payload.Context, (TObject)payload.Command, customData);
        }

        protected abstract Task<bool> CheckIfAuthorizedAsync(
            TAppContext appContext, TContext objContext, TObject obj, TCustomData customData);

        private Task<bool> CheckIfAuthorizedInternal(TAppContext appContext, TContext objContext, TObject obj, object customData)
        {
            if (customData != null && !(customData is TCustomData))
            {
                throw new ArgumentException(
                    $"{GetType()} requires {typeof(TCustomData)} as custom data.",
                    nameof(customData));
            }

            return CheckIfAuthorizedAsync(appContext, objContext, obj, (TCustomData)customData);
        }
    }
}
