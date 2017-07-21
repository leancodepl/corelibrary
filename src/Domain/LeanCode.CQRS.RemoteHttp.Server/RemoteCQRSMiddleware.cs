using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using LeanCode.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public sealed class RemoteCQRSMiddleware
    {
        private readonly TypesCatalog catalog;

        public RemoteCQRSMiddleware(RequestDelegate next, TypesCatalog catalog)
        {
            this.catalog = catalog;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var user = context.User;
            ActionResult actionResult;
            if (request.Method != HttpMethods.Post)
            {
                actionResult = new ActionResult.StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            else if (request.Path.StartsWithSegments("/query"))
            {
                var index = context.RequestServices.GetService<IIndex<TypesCatalog, RemoteQueryHandler>>();
                var queryHandler = index[catalog];
                actionResult = await queryHandler.ExecuteAsync(user, request).ConfigureAwait(false);
            }
            else if (request.Path.StartsWithSegments("/command"))
            {
                var index = context.RequestServices.GetService<IIndex<TypesCatalog, RemoteCommandHandler>>();
                var commandHandler = index[catalog];
                actionResult = await commandHandler.ExecuteAsync(user, request).ConfigureAwait(false);
            }
            else
            {
                actionResult = new ActionResult.StatusCode(StatusCodes.Status404NotFound);
            }

            actionResult.Execute(context);
        }
    }

    public static class RemoteCQRSMiddlewareExtensions
    {
        public static IApplicationBuilder UseRemoteCQRS(this IApplicationBuilder builder, TypesCatalog catalog)
        {
            return builder.UseMiddleware<RemoteCQRSMiddleware>(catalog);
        }

        public static IApplicationBuilder UseRemoteCQRS(this IApplicationBuilder builder, params Assembly[] assemblies)
        {
            return builder.UseRemoteCQRS(new TypesCatalog(assemblies));
        }

        public static IApplicationBuilder UseRemoteCQRS(this IApplicationBuilder builder, params Type[] types)
        {
            return builder.UseRemoteCQRS(new TypesCatalog(types));
        }
    }
}
