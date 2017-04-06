using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class RemoteCQRSMiddlewareCommandsTests
    {
        private readonly StubCommandExecutor command;
        private readonly RemoteCQRSMiddleware middleware;

        public RemoteCQRSMiddlewareCommandsTests()
        {
            command = new StubCommandExecutor();

            var assembly = typeof(RemoteCQRSMiddlewareCommandsTests).GetTypeInfo().Assembly;
            var commandHandler = new RemoteCommandHandler(command, assembly);
            var queryHandler = new RemoteQueryHandler(new StubQueryExecutor(), assembly);
            middleware = new RemoteCQRSMiddleware(assembly, StubIndex.Create(commandHandler), StubIndex.Create(queryHandler));
        }

        [Fact]
        public async Task Writes_NotFound_if_command_cannot_be_found()
        {
            var (status, _) = await Invoke("non.Existing.Command");
            Assert.Equal(StatusCodes.Status404NotFound, status);
        }

        [Fact]
        public async Task Writes_BadRequest_if_body_is_malformed()
        {
            var (status, _) = await Invoke(content: "malformed body 123");
            Assert.Equal(StatusCodes.Status400BadRequest, status);
        }

        [Fact]
        public async Task Writes_NotFound_if_trying_to_execute_non_remote_command()
        {
            var (status, _) = await Invoke(typeof(SampleCommand).FullName);
            Assert.Equal(StatusCodes.Status404NotFound, status);
        }

        [Fact]
        public async Task Does_not_call_CommandExecutor_when_executing_non_remote_query()
        {
            var (status, _) = await Invoke(typeof(SampleCommand).FullName);
            Assert.Null(command.LastCommand);
        }

        [Fact]
        public async Task Deserializes_correct_command_object()
        {
            await Invoke(content: "{'Prop': 12}");

            var q = Assert.IsType<SampleRemoteCommand>(command.LastCommand);
            Assert.Equal(12, q.Prop);
        }

        [Fact]
        public async Task Writes_OK_when_command_has_been_executed()
        {
            var (status, _) = await Invoke();
            Assert.Equal(StatusCodes.Status200OK, status);
        }

        private async Task<(int statusCode, string response)> Invoke(string type = null, string content = "{}", string method = "POST")
        {
            type = type ?? typeof(SampleRemoteCommand).FullName;

            var ctx = new StubContext(method, "/command/" + type, content);
            await middleware.Invoke(ctx);

            var statusCode = ctx.Response.StatusCode;
            var body = (MemoryStream)ctx.Response.Body;
            return (statusCode, Encoding.UTF8.GetString(body.ToArray()));
        }
    }

    public class SampleCommand : ICommand
    { }

    public class SampleRemoteCommand : IRemoteCommand
    {
        public int Prop { get; set; }
    }
}
