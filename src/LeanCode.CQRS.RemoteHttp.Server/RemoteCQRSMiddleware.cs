using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    sealed class RemoteCQRSMiddleware
    {
        private readonly RemoteCommandHandler commandHandler;
        private readonly RemoteQueryHandler queryHandler;

        public RemoteCQRSMiddleware(
            RequestDelegate next,
            Assembly typesAssembly,
            IIndex<Assembly, RemoteCommandHandler> commandHandlerFactory,
            IIndex<Assembly, RemoteQueryHandler> queryHandlerFactory)
        {
            this.commandHandler = commandHandlerFactory[typesAssembly];
            this.queryHandler = queryHandlerFactory[typesAssembly];
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
                actionResult = await queryHandler.ExecuteAsync(request).ConfigureAwait(false);
            }
            else if (request.Path.StartsWithSegments("/command"))
            {
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
