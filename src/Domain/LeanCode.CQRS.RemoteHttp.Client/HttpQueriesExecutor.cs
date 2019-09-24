using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.Json;

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
            using var content = JsonContent.Create(query, query.GetType());
            using var response = await client
                .PostAsync("query/" + query.GetType().FullName, content);

            response.HandleCommonCQRSErrors<QueryNotFoundException, InvalidQueryException>();

            var responseContent = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<TResult>(responseContent, serializerOptions);
        }

        public void Dispose() => client.Dispose();
    }
}
