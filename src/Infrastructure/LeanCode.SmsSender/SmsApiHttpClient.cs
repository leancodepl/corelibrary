using System.Net.Http;

namespace LeanCode.SmsSender
{
    public class SmsApiHttpClient
    {
        public HttpClient Client { get; }

        public SmsApiHttpClient(HttpClient client)
        {
            Client = client;
        }
    }
}
