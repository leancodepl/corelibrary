using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public sealed class RemoteCQRSMiddleware
    {
        private readonly Assembly typesAssembly;

        public RemoteCQRSMiddleware(RequestDelegate next, Assembly typesAssembly)
        {
            this.typesAssembly = typesAssembly;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            IActionResult actionResult;
            if (request.Method != HttpMethods.Post)
            {
                actionResult = new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
            }
            else if (request.Path.StartsWithSegments("/query"))
            {
                var index = context.RequestServices.GetService<IIndex<Assembly, RemoteQueryHandler>>();
                var queryHandler = index[typesAssembly];
                actionResult = await queryHandler.ExecuteAsync(request).ConfigureAwait(false);
            }
            else if (request.Path.StartsWithSegments("/command"))
            {
                var index = context.RequestServices.GetService<IIndex<Assembly, RemoteCommandHandler>>();
                var commandHandler = index[typesAssembly];
                actionResult = await commandHandler.ExecuteAsync(request).ConfigureAwait(false);
            }
            else
            {
                actionResult = new StatusCodeResult(StatusCodes.Status404NotFound);
            }

            actionResult.Execute(context);
        }
    }

    public static class RemoteCQRSMiddlewareExtensions
    {
        public static IApplicationBuilder UseRemoteCQRS(this IApplicationBuilder builder, Assembly typesAssembly)
        {
            return builder.UseMiddleware<RemoteCQRSMiddleware>(typesAssembly);
        }

        public static IApplicationBuilder UseRemoteCQRS(this IApplicationBuilder builder, Type typesAssembly)
        {
            var assembly = typesAssembly.GetTypeInfo().Assembly;
            return builder.UseMiddleware<RemoteCQRSMiddleware>(assembly);
        }
    }
}
