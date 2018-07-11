using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace LeanCode.SmsSender.Tests
{
    public class SmsSenderClientTests
    {
        private static readonly SmsApiConfiguration Config = new SmsApiConfiguration
        {
            Login = "",
            Password = "",
            From = "",
            FastMode = false,
            TestMode = false,
        };

        private readonly SmsApiClient client;

        public SmsSenderClientTests()
        {
            this.client = new SmsApiClient(Config, new SmsApiHttpClient(new HttpClient()));
        }

#pragma warning disable xUnit1004
        [Fact(Skip = "SmsApi credentials required")]
        public async Task Sends_sms_correctly()
        {
            var message = "SmsSender works fine";
            var phoneNumber = "";
            await client.Send(message, phoneNumber);
        }
#pragma warning restore xUnit1004
    }
}
