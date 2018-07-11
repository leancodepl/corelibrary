using System.Net.Http;

namespace LeanCode.PdfGenerator.PdfRocket
{
    public class PdfRocketHttpClient
    {
        public HttpClient Client { get; }

        public PdfRocketHttpClient(HttpClient client)
        {
            Client = client;
        }
    }
}
