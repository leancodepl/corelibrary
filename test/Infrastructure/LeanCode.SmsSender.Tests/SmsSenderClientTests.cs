using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace LeanCode.SmsSender.Tests
{
    public class SmsSenderClientTests
    {
        private static readonly SmsApiConfiguration Config = new SmsApiConfiguration
        {
            Login = string.Empty,
            Password = string.Empty,
            From = string.Empty,
            FastMode = false,
            TestMode = false,
        };

        private readonly SmsApiClient client;

        public SmsSenderClientTests()
        {
            this.client = new SmsApiClient(Config, new SmsApiHttpClient(new HttpClient()));
        }

        [SuppressMessage("?", "xUnit1004", Justification = "Requires custom data.")]
        [Fact(Skip = "SmsApi credentials required")]
        public async Task Sends_sms_correctly()
        {
            var message = "SmsSender works fine";
            var phoneNumber = string.Empty;
            await client.Send(message, phoneNumber);
        }
    }
}
