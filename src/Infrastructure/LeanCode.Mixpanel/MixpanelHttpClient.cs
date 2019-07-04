using System.Net.Http;

namespace LeanCode.Mixpanel
{
    public class MixpanelHttpClient
    {
        public HttpClient Client { get; }

        public MixpanelHttpClient(HttpClient client)
        {
            Client = client;
        }
    }
}
