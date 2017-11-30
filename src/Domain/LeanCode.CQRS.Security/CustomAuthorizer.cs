using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.Security
{
    public interface ICustomAuthorizer<TAppContext>
    {
        Task<bool> CheckIfAuthorized(TAppContext appContext, QueryExecutionPayload payload, object customData);
        Task<bool> CheckIfAuthorized(TAppContext appContext, CommandExecutionPayload payload, object customData);
    }

    public abstract class CustomAuthorizer<TAppContext, TContext, TObject>
        : ICustomAuthorizer<TAppContext>
    {
        public Task<bool> CheckIfAuthorized(
            TAppContext appContext,
            QueryExecutionPayload payload,
            object customData)
        {
            return CheckIfAuthorized(appContext, (TContext)payload.Context, (TObject)payload.Query);
        }

        public Task<bool> CheckIfAuthorized(
            TAppContext appContext,
            CommandExecutionPayload payload,
            object customData)
        {
            return CheckIfAuthorized(appContext, (TContext)payload.Context, (TObject)payload.Command);
        }

        protected abstract Task<bool> CheckIfAuthorized(
            TAppContext appContext, TContext objContext, TObject obj);
    }

    public abstract class CustomAuthorizer<TAppContext, TContext, TObject, TCustomData>
        : ICustomAuthorizer<TAppContext>
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
