using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class HttpQueriesExecutor : IRemoteQueryExecutor, IDisposable
    {
        private readonly JsonSerializerSettings serializerSettings;
        private readonly HttpClient client;

        public HttpQueriesExecutor(
            Uri baseAddress,
            JsonSerializerSettings settings = null)
        {
            client = new HttpClient
            {
                BaseAddress = baseAddress
            };
            serializerSettings = settings;
        }

        public HttpQueriesExecutor(
            Uri baseAddress,
            HttpMessageHandler handler,
            JsonSerializerSettings settings = null)
        {
            client = new HttpClient(handler)
            {
                BaseAddress = baseAddress
            };
            serializerSettings = settings;
        }

        public HttpQueriesExecutor(
            Uri baseAddress,
            HttpMessageHandler handler,
            bool disposeHandler,
            JsonSerializerSettings settings = null)
        {
            client = new HttpClient(handler, disposeHandler)
            {
                BaseAddress = baseAddress
            };
            serializerSettings = settings;
        }

        public virtual async Task<TResult> GetAsync<TResult>(IRemoteQuery<TResult> query)
        {
            var stringified = JsonConvert.SerializeObject(query);
            using (var content = new StringContent(stringified, Encoding.UTF8, "application/json"))
            {
                using (var response = await client.PostAsync("query/" + query.GetType().FullName, content).ConfigureAwait(false))
                {
                    response.HandleCommonCQRSErrors<QueryNotFoundException, InvalidQueryException>();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TResult>(
                        responseContent, serializerSettings);
                }
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
