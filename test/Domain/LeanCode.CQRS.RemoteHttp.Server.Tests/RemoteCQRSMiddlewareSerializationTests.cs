using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class RemoteCQRSMiddlewareSerializationTests : BaseMiddlewareTests
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new AlwaysUpperPolicy(),
        };

        public RemoteCQRSMiddlewareSerializationTests()
            : base("query", typeof(RemoteCQRSMiddlewareTests), new Utf8JsonSerializer(SerializerOptions)) { }

        [Fact]
        public async Task Uses_serialization_options_when_deserializing_payload()
        {
            await Invoke(typeof(SerializationTestQuery).FullName, "{\"PROPERTY\": \"content\"}");

            var query = Assert.IsType<SerializationTestQuery>(Query.LastQuery);
            Assert.Equal("content", query.Property);
        }

        [Fact]
        public async Task Uses_serialization_options_when_serializing_result()
        {
            Query.NextResult = new SerializationTestResult { Result = "content" };
            var (_, content) = await Invoke(typeof(SerializationTestQuery).FullName);

            Assert.Equal("{\"RESULT\":\"content\"}", content);
        }
    }

    internal class AlwaysUpperPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToUpper();
    }

    public class SerializationTestResult
    {
        public string? Result { get; set; }
    }

    public class SerializationTestQuery : IRemoteQuery<SerializationTestResult>
    {
        public string? Property { get; set; }
    }
}
