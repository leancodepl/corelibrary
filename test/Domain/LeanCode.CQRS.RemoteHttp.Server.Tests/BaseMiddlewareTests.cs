using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using NSubstitute;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public abstract class BaseMiddlewareTests
    {
        private readonly TypesCatalog catalog = new TypesCatalog(typeof(BaseMiddlewareTests));
        private readonly IServiceProvider serviceProvider;

        protected readonly StubQueryExecutor query = new StubQueryExecutor();
        protected readonly StubCommandExecutor command = new StubCommandExecutor();
        protected readonly RemoteCQRSMiddleware<AppContext> middleware;

        private readonly string endpoint;
        private readonly string defaultObject;

        public BaseMiddlewareTests(string endpoint, Type defaultObject)
        {
            this.endpoint = endpoint;
            this.defaultObject = defaultObject.FullName;
            this.serviceProvider = Substitute.For<IServiceProvider>();
            this.serviceProvider.GetService(typeof(IQueryExecutor<AppContext>)).Returns(query);
            this.serviceProvider.GetService(typeof(ICommandExecutor<AppContext>)).Returns(command);

            middleware = new RemoteCQRSMiddleware<AppContext>(
                catalog,
                h => new AppContext(h.User),
                null);
        }

        protected async Task<(int statusCode, string response)> Invoke(
            string type = null,
            string content = "{}",
            string method = "POST",
            ClaimsPrincipal user = null)
        {
            type = type ?? defaultObject;

            var ctx = new StubContext(method, $"/{endpoint}/" + type, content);
            ctx.RequestServices = serviceProvider;
            ctx.User = user;
            await middleware.Invoke(ctx);

            var statusCode = ctx.Response.StatusCode;
            var body = (MemoryStream)ctx.Response.Body;
            return (statusCode, Encoding.UTF8.GetString(body.ToArray()));
        }
    }
}
