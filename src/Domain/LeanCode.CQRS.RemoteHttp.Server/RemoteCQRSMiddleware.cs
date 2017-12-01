using System;
using System.Threading.Tasks;
using Autofac;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public sealed class RemoteCQRSMiddleware<TAppContext>
    {
        private readonly TypesCatalog catalog;
        private readonly Func<HttpContext, TAppContext> contextTranslator;

        public RemoteCQRSMiddleware(
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator,
            RequestDelegate next)
        {
            this.catalog = catalog;
            this.contextTranslator = contextTranslator;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            ActionResult actionResult;
            if (request.Method != HttpMethods.Post)
            {
                actionResult = new ActionResult.StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            else if (request.Path.StartsWithSegments("/query"))
            {
                var executor = context.RequestServices.GetService<IQueryExecutor<TAppContext>>();
                var queryHandler = new RemoteQueryHandler<TAppContext>(executor, catalog, contextTranslator);
                actionResult = await queryHandler.ExecuteAsync(context).ConfigureAwait(false);
            }
            else if (request.Path.StartsWithSegments("/command"))
            {
                var executor = context.RequestServices.GetService<ICommandExecutor<TAppContext>>();
                var commandHandler = new RemoteCommandHandler<TAppContext>(executor, catalog, contextTranslator);
                actionResult = await commandHandler.ExecuteAsync(context).ConfigureAwait(false);
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
        public static IApplicationBuilder UseRemoteCQRS<TAppContext>(
            this IApplicationBuilder builder,
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
        {
            return builder.UseMiddleware<RemoteCQRSMiddleware<TAppContext>>(catalog, contextTranslator);
        }
    }
}
