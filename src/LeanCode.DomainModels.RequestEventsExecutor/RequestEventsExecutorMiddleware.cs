using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.RequestEventsExecutor
{
    class RequestEventsExecutorMiddleware
    {
        private readonly RequestDelegate next;

        public RequestEventsExecutorMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var eventExecutor = httpContext.RequestServices.GetService<EventExecutor>();
            return eventExecutor.Execute(next, httpContext);
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
