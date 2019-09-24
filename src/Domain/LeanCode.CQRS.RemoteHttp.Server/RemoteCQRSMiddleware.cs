using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public sealed class RemoteCQRSMiddleware<TAppContext>
    {
        private static readonly byte[] NullString = Encoding.UTF8.GetBytes("null");

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

        private async Task ExecuteResult(ExecutionResult result, HttpContext context)
        {
            if (result.Skipped)
            {
                await next(context);
            }
            else
            {
                context.Response.StatusCode = result.StatusCode;
                if (result.Succeeded)
                {
                    context.Response.ContentType = "application/json";
                    if (result.Payload is null)
                    {
                        await context.Response.Body.WriteAsync(NullString);
                    }
                    else
                    {
                        await JsonSerializer.SerializeAsync(context.Response.Body, result.Payload, result.Payload.GetType());
                    }
                }
            }
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
