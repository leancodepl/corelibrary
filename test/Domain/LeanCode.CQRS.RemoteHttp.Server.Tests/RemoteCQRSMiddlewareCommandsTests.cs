using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class RemoteCQRSMiddlewareCommandsTests : BaseMiddlewareTests
    {
        public RemoteCQRSMiddlewareCommandsTests()
            : base("command", typeof(SampleRemoteCommand))
        { }

        [Fact]
        public async Task Passes_execution_further_if_cannot_find_command_type()
        {
            var (status, _) = await Invoke("non.Existing.Command");
            Assert.Equal(PipelineContinued, status);
        }

        [Fact]
        public async Task Returns_BadRequest_if_body_is_malformed()
        {
            var (status, _) = await Invoke(content: "malformed body 123");
            Assert.Equal(StatusCodes.Status400BadRequest, status);
        }

        [Fact]
        public async Task Returns_NotFound_if_trying_to_execute_non_remote_command()
        {
            var (status, _) = await Invoke(typeof(SampleCommand).FullName);
            Assert.Equal(StatusCodes.Status404NotFound, status);
        }

        [Fact]
        public async Task Does_not_call_CommandExecutor_when_executing_non_remote_query()
        {
            await Invoke(typeof(SampleCommand).FullName);
            Assert.Null(Command.LastCommand);
        }

        [Fact]
        public async Task Deserializes_correct_command_object()
        {
            await Invoke(content: "{'Prop': 12}");

            var q = Assert.IsType<SampleRemoteCommand>(Command.LastCommand);
            Assert.Equal(12, q.Prop);
        }

        [Fact]
        public async Task Returns_OK_when_command_has_been_executed()
        {
            var (status, _) = await Invoke();
            Assert.Equal(StatusCodes.Status200OK, status);
        }

        [Fact]
        public async Task Returns_successful_command_result_when_command_has_been_executed()
        {
            var (_, content) = await Invoke();

            var result = JsonConvert.DeserializeObject<CommandResult>(content);
            Assert.True(result.WasSuccessful);
        }

        [Fact]
        public async Task Returns_UnprocessableEntity_when_command_does_not_pass_validation()
        {
            var (status, _) = await Invoke(content: "{'Prop': 999}");

            Assert.Equal(StatusCodes.Status422UnprocessableEntity, status);
        }

        [Fact]
        public async Task Returns_CommandResult_with_errors_when_command_does_not_pass_validation()
        {
            var (_, content) = await Invoke(content: "{'Prop': 999}");

            var result = JsonConvert.DeserializeObject<CommandResult>(content);
            Assert.False(result.WasSuccessful);
            var err = Assert.Single(result.ValidationErrors);
            Assert.Equal(StubCommandExecutor.SampleError.PropertyName, err.PropertyName);
            Assert.Equal(StubCommandExecutor.SampleError.ErrorCode, err.ErrorCode);
            Assert.Equal(StubCommandExecutor.SampleError.ErrorMessage, err.ErrorMessage);
        }

        [Fact]
        public async Task Correctly_passes_the_app_context()
        {
            var user = new ClaimsPrincipal();

            await Invoke(user: user);

            Assert.NotNull(Command.LastAppContext);
            Assert.Equal(user, Command.LastAppContext.User);
        }
    }

    public class SampleCommand : ICommand
    { }

    public class SampleRemoteCommand : IRemoteCommand
    {
        public int Prop { get; set; }
    }

    public class SampleRemoteCommand2 : IRemoteCommand
    {
    }
}
