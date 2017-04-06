using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class RemoteCQRSMiddlewareTests
    {
        private readonly RemoteCQRSMiddleware middleware;

        public RemoteCQRSMiddlewareTests()
        {
            var assembly = typeof(RemoteCQRSMiddlewareQueriesTests).GetTypeInfo().Assembly;
            var command = new RemoteCommandHandler(new StubCommandExecutor(), assembly);
            var query = new RemoteQueryHandler(new StubQueryExecutor(), assembly);
            middleware = new RemoteCQRSMiddleware(null, assembly, StubIndex.Create(command), StubIndex.Create(query));
        }

        [Fact]
        public async Task Writes_MethodNotAllowed_if_using_PUT()
        {
            var (status, _) = await Invoke("/query", method: "PUT");
            Assert.Equal(StatusCodes.Status405MethodNotAllowed, status);
        }

        [Fact]
        public async Task Writes_NotFound_if_path_does_not_start_with_query_nor_command()
        {
            var (status, _) = await Invoke("/non-query");
            Assert.Equal(StatusCodes.Status404NotFound, status);
        }

        private async Task<(int statusCode, string response)> Invoke(string path, string content = "{}", string method = "POST")
        {
            var ctx = new StubContext(method, path, content);
            await middleware.Invoke(ctx);

            var statusCode = ctx.Response.StatusCode;
            var body = (MemoryStream)ctx.Response.Body;
            return (statusCode, Encoding.UTF8.GetString(body.ToArray()));
        }
    }
}
