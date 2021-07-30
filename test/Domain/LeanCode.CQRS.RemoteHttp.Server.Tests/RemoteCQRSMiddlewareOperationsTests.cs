using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class RemoteCQRSMiddlewareOperationsTests : BaseMiddlewareTests
    {
        public RemoteCQRSMiddlewareOperationsTests()
            : base("operation", typeof(SampleRemoteOperation)) { }

        [Fact]
        public async Task Passes_execution_further_if_operation_type_cannot_be_found()
        {
            var (status, _) = await Invoke("non.Existing.Operation");
            Assert.Equal(PipelineContinued, status);
        }

        [Fact]
        public async Task Writes_BadRequest_if_body_is_malformed()
        {
            var (status, _) = await Invoke(content: "malformed body 123");
            Assert.Equal(StatusCodes.Status400BadRequest, status);
        }

        [Fact]
        public async Task Writes_NotFound_if_trying_to_execute_non_remote_operation()
        {
            var (status, _) = await Invoke(typeof(SampleOperation).FullName);
            Assert.Equal(StatusCodes.Status404NotFound, status);
        }

        [Fact]
        public async Task Does_not_call_OperationExecutor_when_executing_non_remote_operation()
        {
            await Invoke(typeof(SampleOperation).FullName);
            Assert.Null(Operation.LastOperation);
        }

        [Fact]
        public async Task Deserializes_correct_operation_object()
        {
            await Invoke(content: @"{""Prop"": 12}");

            var q = Assert.IsType<SampleRemoteOperation>(Operation.LastOperation);
            Assert.Equal(12, q.Prop);
        }

        [Fact]
        public async Task Writes_OK_when_operation_has_been_executed()
        {
            var (status, _) = await Invoke();
            Assert.Equal(StatusCodes.Status200OK, status);
        }

        [Fact]
        public async Task Serializes_null_as_null_not_empty_response()
        {
            var (_, content) = await Invoke(typeof(NullReturningOperation).FullName);
            Assert.Equal("null", content);
        }

        [Fact]
        public async Task Writes_result_when_operation_has_been_executed()
        {
            var (_, content) = await Invoke();
            Assert.Equal("0", content);
        }

        [Fact]
        public async Task Correctly_passes_the_app_context()
        {
            var user = new ClaimsPrincipal();

            await Invoke(user: user);

            Assert.NotNull(Operation.LastAppContext);
            Assert.Equal(user, Operation.LastAppContext!.User);
        }
    }

    public class SampleOperation : IOperation<int> { }

    public class SampleRemoteOperation : IRemoteOperation<int>
    {
        public int Prop { get; set; }
    }

    public class SampleRemoteOperation2 : IRemoteOperation<int>
    {
        public int Prop { get; set; }
    }

    public class NullReturningOperation : IRemoteOperation<object> { }
}
