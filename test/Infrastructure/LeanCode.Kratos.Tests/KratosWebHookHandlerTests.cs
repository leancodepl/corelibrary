using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace LeanCode.Kratos.Tests;

public class KratosWebHookHandlerTests
{
    private sealed class KratosWebHookTestHandler : KratosWebHookHandlerBase
    {
        private readonly Func<HttpContext, Task> handler;

        public new string ApiKeyHeaderName => base.ApiKeyHeaderName;

        public KratosWebHookTestHandler(KratosWebHookHandlerConfig config, Func<HttpContext, Task> handler)
            : base(config)
        {
            this.handler = handler;
        }

        protected override Task HandleCoreAsync(HttpContext ctx) => handler(ctx);
    }

    private static readonly string ApiKey = Guid.NewGuid().ToString();

    private static KratosWebHookTestHandler PrepareHandler(Func<HttpContext, Task> handler)
    {
        return new(new(ApiKey), handler);
    }

    private static KratosWebHookTestHandler PrepareHandler(Action<HttpContext> handler)
    {
        return PrepareHandler(ctx =>
        {
            handler(ctx);
            return Task.CompletedTask;
        });
    }

    private static async Task<HttpContext> RunAsync(
        KratosWebHookTestHandler handler,
        Dictionary<string, StringValues> headers
    )
    {
        var features = new FeatureCollection();
        features.Set<IHttpRequestFeature>(new HttpRequestFeature() { Headers = new HeaderDictionary(headers) });
        features.Set<IHttpResponseFeature>(new HttpResponseFeature());
        var httpContext = new DefaultHttpContext(features);
        await handler.HandleAsync(httpContext);
        return httpContext;
    }

    [Fact]
    public async Task Responds_with_status_code_403_without_running_inner_handler_if_api_key_is_invalid()
    {
        var handler = PrepareHandler(ctx => throw new UnreachableException());
        var ctx = await RunAsync(handler, new() { [handler.ApiKeyHeaderName] = Guid.NewGuid().ToString() });

        Assert.Equal(403, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task Runs_inner_handler_if_api_key_is_valid()
    {
        var handler = PrepareHandler(ctx => ctx.Response.StatusCode = 200);
        var ctx = await RunAsync(handler, new() { [handler.ApiKeyHeaderName] = ApiKey });

        Assert.Equal(200, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task Swallows_unhandled_exceptions_thrown_by_inner_handler_and_responds_with_status_code_500()
    {
        var handler = PrepareHandler(ctx => throw new InvalidOperationException());
        var ctx = await RunAsync(handler, new() { [handler.ApiKeyHeaderName] = ApiKey });

        Assert.Equal(500, ctx.Response.StatusCode);
    }
}
