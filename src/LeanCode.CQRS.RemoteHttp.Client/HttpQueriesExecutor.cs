using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public sealed class HttpQueriesExecutor : IRemoteQueryExecutor, IDisposable
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
                using (var response = await client.PostAsync("query/" + query.GetType().FullName, content))
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new QueryNotFoundException();
                    }
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new InvalidQueryException();
                    }
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        throw new InternalServerErrorException();
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
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
