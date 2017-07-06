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
                if (ShouldProcess(context.Response))
                {
                    return ExecutionResult.Process();
                }
                else
                {
                    logger.Warning(
                        "Execution of request to {Path} resulted in {StatusCode} status code, skipping event execution",
                        context.Request.Path, context.Response.StatusCode);
                    return ExecutionResult.Skip();
                }
            });
        }

        private static bool ShouldProcess(HttpResponse response)
        {
            if (response != null)
            {
                var code = response.StatusCode;
                return code >= 100 && code < 400;
            }
            else
            {
                return true;
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
