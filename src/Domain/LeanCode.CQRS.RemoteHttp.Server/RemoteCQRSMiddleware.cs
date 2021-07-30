using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public sealed class RemoteCQRSMiddleware<TAppContext>
    {
        private static readonly byte[] NullString = Encoding.UTF8.GetBytes("null");

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<RemoteCQRSMiddleware<TAppContext>>();
        private readonly TypesCatalog catalog;
        private readonly Func<HttpContext, TAppContext> contextTranslator;
        private readonly ISerializer serializer;
        private readonly RequestDelegate next;

        public RemoteCQRSMiddleware(
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator,
            ISerializer serializer,
            RequestDelegate next)
        {
            this.catalog = catalog;
            this.contextTranslator = contextTranslator;
            this.serializer = serializer;
            this.next = next;
        }

        public RemoteCQRSMiddleware(
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator,
            RequestDelegate next)
        {
            this.catalog = catalog;
            this.contextTranslator = contextTranslator;
            this.next = next;

            serializer = new Utf8JsonSerializer();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;

            ExecutionResult result;

            if (!HttpMethods.IsPost(request.Method))
            {
                result = ExecutionResult.Fail(StatusCodes.Status405MethodNotAllowed);
            }
            else if (request.Path.StartsWithSegments("/query", StringComparison.InvariantCulture))
            {
                var queryHandler = new RemoteQueryHandler<TAppContext>(
                    context.RequestServices, catalog, contextTranslator, serializer);

                result = await queryHandler.ExecuteAsync(context);
            }
            else if (request.Path.StartsWithSegments("/command", StringComparison.InvariantCulture))
            {
                var commandHandler = new RemoteCommandHandler<TAppContext>(
                    context.RequestServices, catalog, contextTranslator, serializer);

                result = await commandHandler.ExecuteAsync(context);
            }
            else if (request.Path.StartsWithSegments("/operation"))
            {
                var operationHandler = new RemoteOperationHandler<TAppContext>(
                    context.RequestServices, catalog, contextTranslator, serializer);

                result = await operationHandler.ExecuteAsync(context);
            }
            else
            {
                result = ExecutionResult.Skip;
            }

            await ExecuteResultAsync(result, context);
        }

        private async Task ExecuteResultAsync(ExecutionResult result, HttpContext context)
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
                        try
                        {
                            await serializer.SerializeAsync(
                                context.Response.Body,
                                result.Payload,
                                result.Payload.GetType(),
                                context.RequestAborted);
                        }
                        catch (Exception ex)
                            when (ex is OperationCanceledException || ex.InnerException is OperationCanceledException)
                        {
                            logger.Warning(ex, "Failed to serialize response, request aborted");
                        }
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
            return builder.UseMiddleware<RemoteCQRSMiddleware<TAppContext>>(
                catalog,
                contextTranslator,
                new Utf8JsonSerializer(
                    new JsonSerializerOptions
                    {
                        Converters =
                        {
                            new JsonDateOnlyConverter(),
                            new JsonTimeOnlyConverter(),
                        },
                    }));
        }

        public static IApplicationBuilder UseRemoteCQRS<TAppContext>(
            this IApplicationBuilder builder,
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator,
            ISerializer serializer)
        {
            return builder.UseMiddleware<RemoteCQRSMiddleware<TAppContext>>(catalog, contextTranslator, serializer);
        }
    }
}
