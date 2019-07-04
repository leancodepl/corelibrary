using System.Net.Http;

namespace LeanCode.Facebook
{
    public class FacebookHttpClient
    {
        public HttpClient Client { get; }

        public FacebookHttpClient(HttpClient client)
        {
            Client = client;
        }
    }
}
