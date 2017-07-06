using System.Threading.Tasks;
using LeanCode.DomainModels.EventsExecutor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.RequestEventsExecutor
{
    class RequestEventsExecutorMiddleware
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<RequestEventsExecutorMiddleware>();

        private readonly RequestDelegate next;

        public RequestEventsExecutorMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var eventExecutor = context.RequestServices.GetService<IEventsExecutor>();
            return eventExecutor.HandleEventsOf(async () =>
            {
                logger.Debug("Executing request to {Path} and capturing its events",
                    context.Request.Path);
                await next(context);
                if (ShouldAbort(context.Response))
                {
                    logger.Warning(
                        "Execution of request to {Path} resulted in {StatusCode} status code, skipping event execution",
                        context.Request.Path, context.Response.StatusCode);
                    return ExecutionResult.Skip();
                }
                else
                {
                    return ExecutionResult.Process();
                }
            });
        }

        private static bool ShouldAbort(HttpResponse response)
        {
            if (response != null)
            {
                var code = response.StatusCode;
                return code >= 100 && code < 400;
            }
            else
            {
                return false;
            }
        }
    }

    public static class RequestEventsExecutorMiddlewareExtensions
    {
        public static IApplicationBuilder UseEventsExecutor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestEventsExecutorMiddleware>();
        }
    }
}
