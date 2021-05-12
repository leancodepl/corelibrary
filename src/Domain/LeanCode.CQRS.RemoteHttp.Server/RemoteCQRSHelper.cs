using System;
using System.Text;
using System.Threading.Tasks;
using LeanCode.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public static class RemoteCQRSHelper
    {
        private static readonly byte[] NullString = Encoding.UTF8.GetBytes("null");

        public static IApplicationBuilder UseRemoteCQRS<TAppContext>(
            this IApplicationBuilder builder,
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
        {
            return builder
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    foreach (var assembly in catalog.Assemblies)
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            endpoints.MapPost($"/command/{type.FullName}", async context =>
                            {
                                var commandHandler = new RemoteCommandHandler<TAppContext>(
                                    context.RequestServices, type, contextTranslator, new Utf8JsonSerializer());

                                var result = await commandHandler.ExecuteAsync(context);
                                await ExecuteResultAsync(result, context, new Utf8JsonSerializer());
                            });
                            endpoints.MapPost($"/query/{type.FullName}", async context =>
                            {
                                var queryHandler = new RemoteQueryHandler<TAppContext>(
                                    context.RequestServices, type, contextTranslator, new Utf8JsonSerializer());

                                var result = await queryHandler.ExecuteAsync(context);
                                await ExecuteResultAsync(result, context, new Utf8JsonSerializer());
                            });
                        }
                    }
                });
        }

        public static IApplicationBuilder UseRemoteCQRS<TAppContext>(
            this IApplicationBuilder builder,
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator,
            ISerializer serializer)
        {
            return builder
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    foreach (var assembly in catalog.Assemblies)
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            endpoints.MapPost($"/command/{type.FullName}", async context =>
                            {
                                var commandHandler = new RemoteCommandHandler<TAppContext>(
                                    context.RequestServices, type, contextTranslator, serializer);

                                var result = await commandHandler.ExecuteAsync(context);
                                await ExecuteResultAsync(result, context, serializer);
                            });
                            endpoints.MapPost($"/query/{type.FullName}", async context =>
                            {
                                var queryHandler = new RemoteQueryHandler<TAppContext>(
                                    context.RequestServices, type, contextTranslator, serializer);

                                var result = await queryHandler.ExecuteAsync(context);
                                await ExecuteResultAsync(result, context, serializer);
                            });
                        }
                    }
                });
        }

        private static async Task ExecuteResultAsync(ExecutionResult result, HttpContext context, ISerializer serializer)
        {
            if (result.Skipped)
            {
                await Task.CompletedTask;
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
                            Serilog.Log.Warning(ex, "Failed to serialize response, request aborted");
                        }
                    }
                }
            }
        }
    }
}
