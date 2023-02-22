using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using NSubstitute;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests;

public abstract class BaseMiddlewareTests
{
    protected const int PipelineContinued = 100;

    private readonly TypesCatalog catalog = new(typeof(BaseMiddlewareTests));
    private readonly IServiceProvider serviceProvider;

    protected StubQueryExecutor Query { get; } = new();
    protected StubCommandExecutor Command { get; } = new();
    protected StubOperationExecutor Operation { get; } = new();
    protected RemoteCQRSMiddleware<AppContext> Middleware { get; }

    private readonly string endpoint;
    private readonly string defaultObject;

    protected BaseMiddlewareTests(string endpoint, Type defaultObject, ISerializer? serializer = null)
    {
        this.endpoint = endpoint;
        this.defaultObject = defaultObject.FullName!;

        serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IQueryExecutor<AppContext>)).Returns(Query);
        serviceProvider.GetService(typeof(ICommandExecutor<AppContext>)).Returns(Command);
        serviceProvider.GetService(typeof(IOperationExecutor<AppContext>)).Returns(Operation);

        Middleware = new RemoteCQRSMiddleware<AppContext>(
            catalog,
            h => new AppContext(h.User),
            serializer ?? new Utf8JsonSerializer(),
            ctx =>
            {
                ctx.Response.StatusCode = PipelineContinued;
                return Task.CompletedTask;
            }
        );
    }

    protected async Task<(int statusCode, string response)> Invoke(
        string? type = null,
        string content = "{}",
        string method = "POST",
        ClaimsPrincipal? user = null
    )
    {
        type = type ?? defaultObject;

        var ctx = new StubContext(method, $"/{endpoint}/{type}", content)
        {
            RequestServices = serviceProvider,
            User = user ?? new ClaimsPrincipal(),
        };
        await Middleware.InvokeAsync(ctx);

        var statusCode = ctx.Response.StatusCode;
        var body = (MemoryStream)ctx.Response.Body;
        return (statusCode, Encoding.UTF8.GetString(body.ToArray()));
    }
}
