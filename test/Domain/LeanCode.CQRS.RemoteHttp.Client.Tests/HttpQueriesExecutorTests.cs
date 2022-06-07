using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LeanCode.Contracts;
using Xunit;

namespace LeanCode.CQRS.RemoteHttp.Client.Tests
{
    public class HttpQueriesExecutorTests
    {
        [Fact]
        public async Task Correctly_builds_request_path()
        {
            var (exec, handler) = Prepare(HttpStatusCode.OK, "{}");

            await exec.GetAsync(new ExampleQuery());

            Assert.NotNull(handler.Request);
            Assert.Equal(
                new Uri("http://localhost/query/" + typeof(ExampleQuery).FullName),
                handler.Request!.RequestUri);
        }

        [Fact]
        public async Task Serializes_the_request_payload()
        {
            var (exec, handler) = Prepare(HttpStatusCode.OK, "{}");

            await exec.GetAsync(new ExampleQuery { RequestData = "data" });

            Assert.NotNull(handler.Request);
            var content = await handler!.Request!.Content!.ReadAsStringAsync();
            Assert.Equal("{\"RequestData\":\"data\"}", content);
        }

        [Fact]
        public async Task Correctly_deserializes_the_data()
        {
            var (exec, _) = Prepare(HttpStatusCode.OK, "{\"Data\":\"data\"}");

            var result = await exec.GetAsync(new ExampleQuery());

            Assert.NotNull(result);
            Assert.Equal("data", result.Data);
        }

        [Fact]
        public async Task Correctly_deserializes_null_result()
        {
            var (exec, _) = Prepare(HttpStatusCode.OK, "null");

            var result = await exec.GetAsync(new ExampleQuery());

            Assert.Null(result);
        }

        [Fact]
        public async Task Handles_unauthorized()
        {
            await TestExceptionAsync<UnauthorizedException>(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Handles_forbidden()
        {
            await TestExceptionAsync<ForbiddenException>(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Handles_internal_server_errors()
        {
            await TestExceptionAsync<InternalServerErrorException>(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Handles_not_found()
        {
            await TestExceptionAsync<QueryNotFoundException>(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Handles_malformed()
        {
            await TestExceptionAsync<InvalidQueryException>(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Handles_other_errors()
        {
            await TestExceptionAsync<HttpCallErrorException>(HttpStatusCode.BadGateway);
        }

        [Fact]
        public async Task Wraps_JsonException_in_MalformedResponseException()
        {
            var (exec, _) = Prepare(HttpStatusCode.OK, "[{\"");

            await Assert.ThrowsAsync<MalformedResponseException>(() => exec.GetAsync(new ExampleQuery()));
        }

        private static async Task TestExceptionAsync<TException>(HttpStatusCode statusCode)
            where TException : Exception
        {
            var (exec, _) = Prepare(statusCode, "");

            await Assert.ThrowsAsync<TException>(() => exec.GetAsync(new ExampleQuery()));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2000", Justification = "References don't go out of scope.")]
        private static (HttpQueriesExecutor, ShortcircuitingJsonHandler) Prepare(HttpStatusCode statusCode, string result)
        {
            var handler = new ShortcircuitingJsonHandler(statusCode, result);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            return (new HttpQueriesExecutor(client), handler);
        }
    }

    public class ExampleDTO
    {
        public string? Data { get; set; }
    }

    public class ExampleQuery : IQuery<ExampleDTO>
    {
        public string? RequestData { get; set; }
    }
}
