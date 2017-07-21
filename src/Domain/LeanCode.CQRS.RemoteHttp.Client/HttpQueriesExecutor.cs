using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class HttpQueriesExecutor : IRemoteQueryExecutor, IDisposable
    {
        private readonly HttpClient client;

        public HttpQueriesExecutor(Uri baseAddress)
        {
            client = new HttpClient
            {
                BaseAddress = baseAddress
            };
        }

        public HttpQueriesExecutor(Uri baseAddress, HttpMessageHandler handler)
        {
            client = new HttpClient(handler)
            {
                BaseAddress = baseAddress
            };
        }

        public HttpQueriesExecutor(Uri baseAddress, HttpMessageHandler handler, bool disposeHandler)
        {
            client = new HttpClient(handler, disposeHandler)
            {
                BaseAddress = baseAddress
            };
        }

        public async Task<TResult> GetAsync<TResult>(IRemoteQuery<TResult> query)
        {
            var stringified = JsonConvert.SerializeObject(query);
            using (var content = new StringContent(stringified, Encoding.UTF8, "application/json"))
            {
                using (var response = await client.PostAsync("query/" + query.GetType().FullName, content).ConfigureAwait(false))
                {
                    response.HandleCommonCQRSErrors<QueryNotFoundException, InvalidQueryException>();

                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<TResult>(responseContent);
                }
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
