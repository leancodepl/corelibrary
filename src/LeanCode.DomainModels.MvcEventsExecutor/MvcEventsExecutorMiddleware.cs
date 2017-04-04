using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LeanCode.DomainModels.MvcEventsExecutor
{
    class MvcEventsExecutorMiddleware
    {
        private readonly EventExecutor eventExecutor;

        private readonly RequestDelegate next;

        public MvcEventsExecutorMiddleware(
            EventExecutor eventExecutor,
            RequestDelegate next)
        {
            this.eventExecutor = eventExecutor;
            this.next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            return eventExecutor.Execute(next, httpContext);
        }
    }

    public static class MvcEventsExecutorMiddlewareExtensions
    {
        public static IApplicationBuilder UseEventsExecutor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MvcEventsExecutorMiddleware>();
        }
    }
}
