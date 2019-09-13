using System;
using System.IO;
using System.Threading.Tasks;
using LeanCode.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public sealed class RemoteCQRSMiddleware<TAppContext>
    {
        private readonly TypesCatalog catalog;
        private readonly Func<HttpContext, TAppContext> contextTranslator;
        private readonly RequestDelegate next;

        public RemoteCQRSMiddleware(
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator,
            RequestDelegate next)
        {
            this.catalog = catalog;
            this.contextTranslator = contextTranslator;
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            ExecutionResult result;

            if (!HttpMethods.IsPost(request.Method))
            {
                result = ExecutionResult.Fail(StatusCodes.Status405MethodNotAllowed);
            }
            else if (request.Path.StartsWithSegments("/query"))
            {
                var queryHandler = new RemoteQueryHandler<TAppContext>(
                    context.RequestServices, catalog, contextTranslator);

                result = await queryHandler.ExecuteAsync(context).ConfigureAwait(false);
            }
            else if (request.Path.StartsWithSegments("/command"))
            {
                var commandHandler = new RemoteCommandHandler<TAppContext>(
                    context.RequestServices, catalog, contextTranslator);

                result = await commandHandler.ExecuteAsync(context).ConfigureAwait(false);
            }
            else
            {
                result = ExecutionResult.Skip;
            }

            await ExecuteResult(result, context);
        }

        private Task ExecuteResult(ExecutionResult result, HttpContext context)
        {
            if (result.Skipped)
            {
                return next(context);
            }

            context.Response.StatusCode = result.StatusCode;

            if (result.Succeeded)
            {
                context.Response.ContentType = "application/json";

                using var writer = new StreamWriter(context.Response.Body);

                new JsonSerializer().Serialize(writer, result.Payload);
            }

            return Task.CompletedTask;
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
