using System.Net.Http;

namespace LeanCode.EmailSender.SendGrid
{
    public class SendGridHttpClient
    {
        public HttpClient Client { get; }

        public SendGridHttpClient(HttpClient client)
        {
            this.Client = client;
        }
    }
}
