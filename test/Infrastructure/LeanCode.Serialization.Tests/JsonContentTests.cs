using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LeanCode.Serialization.Tests
{
    public class JsonContentTests
    {
        private static readonly Payload SamplePayload = new Payload { Data = "abcd1234" };
        private static readonly string SampleJson = ToJson(SamplePayload);

        [Fact]
        public async Task Correctly_serializes_the_payload_if_read_as_string()
        {
            using var content = JsonContent.Create(SamplePayload);

            var res = await content.ReadAsStringAsync();

            Assert.Equal(SampleJson, res);
        }

        [Fact]
        public async Task Correctly_serializes_the_payload_if_read_as_stream()
        {
            using var content = JsonContent.Create(SamplePayload);
            await using var res = await content.ReadAsStreamAsync();

            var data = new byte[SampleJson.Length];
            var read = await res.ReadAsync(data);

            Assert.Equal(data.Length, read);
            Assert.Equal(Encoding.UTF8.GetBytes(SampleJson), data);
        }

        [Fact]
        public async Task Correctly_serializes_the_payload_if_read_as_bytearray()
        {
            using var content = JsonContent.Create(SamplePayload);

            var res = await content.ReadAsByteArrayAsync();

            Assert.Equal(SampleJson.Length, res.Length);
            Assert.Equal(Encoding.UTF8.GetBytes(SampleJson), res);
        }

        [Fact]
        public async Task Respects_the_serializer_options()
        {
            var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            using var content = JsonContent.Create(SamplePayload, opts);

            var res = await content.ReadAsStringAsync();

            Assert.Equal(ToJson(SamplePayload, "data"), res);
        }

        [Fact]
        public void Has_correct_content_type()
        {
            using var content = JsonContent.Create(SamplePayload);

            Assert.Equal("application/json", content.Headers.ContentType.MediaType);
            Assert.Equal("utf-8", content.Headers.ContentType.CharSet);
        }

        [Fact]
        public void Forces_chunk_encoding()
        {
            using var content = JsonContent.Create(SamplePayload);

            Assert.Null(content.Headers.ContentLength);
        }

        private static string ToJson(Payload payload, string fieldName = nameof(Payload.Data))
        {
            return @$"{{""{fieldName}"":""{payload.Data}""}}";
        }

        public class Payload
        {
            public string Data { get; set; }
        }
    }
}
