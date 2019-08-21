using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
                var queryHandler = new RemoteQueryHandler<TAppContext>(context.RequestServices, catalog, contextTranslator);
                result = await queryHandler.ExecuteAsync(context).ConfigureAwait(false);
            }
            else if (request.Path.StartsWithSegments("/command"))
            {
                var commandHandler = new RemoteCommandHandler<TAppContext>(context.RequestServices, catalog, contextTranslator);
                result = await commandHandler.ExecuteAsync(context).ConfigureAwait(false);
            }
            else
            {
                result = ExecutionResult.Skip();
            }

            await ExecuteResult(result, context);
        }

        private Task ExecuteResult(ExecutionResult result, HttpContext context)
        {
            if (result.Skipped)
            {
                return next(context);
            }
            else if (result.Succeeded)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = result.StatusCode;
                using (var writer = new StreamWriter(context.Response.Body))
                {
                    new JsonSerializer().Serialize(writer, result.Payload);
                }
                return Task.CompletedTask;
            }
            else
            {
                context.Response.StatusCode = result.StatusCode;
                return Task.CompletedTask;
            }
        }
    }

    public static class RemoteCQRSMiddlewareExtensions
    {
        /// <summary>
        /// Adds the <c>RemoteCQRSMiddleware&lt;TAppContext&gt;</c> middleware, which allows RemoteCQRS execution
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <param name="catalog">
        /// Asemblies containing <see cref="IRemoteCommand{TContext}"/> and <see cref="IRemoteQuery{TContext, TResult}"/>
        /// to be exposed via remote CQRS
        /// </param>
        /// <param name="contextTranslator">
        /// Mapper from <see cref="HttpContext" /> to <typeparamref name="TAppContext"/> application context
        /// </param>
        public static IApplicationBuilder UseRemoteCQRS<TAppContext>(
            this IApplicationBuilder builder,
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
        {
            return builder.UseMiddleware<RemoteCQRSMiddleware<TAppContext>>(catalog, contextTranslator);
        }
    }
}
