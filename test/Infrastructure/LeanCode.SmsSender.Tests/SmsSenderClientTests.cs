using System.Threading.Tasks;
using Xunit;

namespace LeanCode.SmsSender.Tests
{
    public class SmsSenderClientTests
    {
        private static readonly SmsSenderConfiguration Config = new SmsSenderConfiguration
        {
            Login = "",
            Password = "",
            From = "",
            FastMode = false,
            TestMode = false,
        };

        private readonly SmsSenderClient client;

        public SmsSenderClientTests()
        {
            this.client = new SmsSenderClient(Config);
        }

#pragma warning disable xUnit1004
        [Fact(Skip = "SmsApi credentials required")] 
        public async Task Sends_sms_correctly()
        {
            var message = "SmsSender works fine";
            var phoneNumber = "";
            await client.Send(message,phoneNumber);
        }
#pragma warning restore xUnit1004
    }
    
}