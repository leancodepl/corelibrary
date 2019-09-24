using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class HttpQueriesExecutor : IRemoteQueryExecutor, IDisposable
    {
        private readonly HttpClient client;
        private readonly JsonSerializerOptions? serializerOptions;

        public HttpQueriesExecutor(Uri baseAddress, JsonSerializerOptions? options = null)
        {
            client = new HttpClient()
            {
                BaseAddress = baseAddress,
            };

            serializerOptions = options;
        }

        public HttpQueriesExecutor(
            Uri baseAddress,
            HttpMessageHandler handler,
            JsonSerializerOptions? options = null)
        {
            client = new HttpClient(handler)
            {
                BaseAddress = baseAddress,
            };

            serializerOptions = options;
        }

        public HttpQueriesExecutor(
            Uri baseAddress,
            HttpMessageHandler handler,
            bool disposeHandler,
            JsonSerializerOptions? options = null)
        {
            client = new HttpClient(handler, disposeHandler)
            {
                BaseAddress = baseAddress,
            };

            serializerOptions = options;
        }

        public virtual async Task<TResult> GetAsync<TResult>(IRemoteQuery<TResult> query)
        {
            using var content = PrepareContent(query);
            using var response = await client
                .PostAsync("query/" + query.GetType().FullName, content)
                .ConfigureAwait(false);

            response.HandleCommonCQRSErrors<QueryNotFoundException, InvalidQueryException>();

            var responseContent = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<TResult>(responseContent, serializerOptions);
        }

        public void Dispose() => client.Dispose();

        private ByteArrayContent PrepareContent<TResult>(IRemoteQuery<TResult> query)
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(query, query.GetType(), serializerOptions);
            var content = new ByteArrayContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = Encoding.UTF8.WebName,
            };
            return content;
        }
    }
}
