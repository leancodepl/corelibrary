using System;
using System.Net.Http;
using System.Threading.Tasks;
using LeanCode.SmsSender.Exceptions;
using Xunit;

namespace LeanCode.SmsSender.Tests
{
    public class SmsSenderClientTests
    {
        private static readonly string Token = Environment.GetEnvironmentVariable("SMSAPI_TOKEN");
        private static readonly string PhoneNumber = Environment.GetEnvironmentVariable("SMSAPI_PHONENUMBERTO");
        private static readonly string Message = "SmsSender works fine";

        private static readonly SmsApiConfiguration Config = new(Token, "")
        {
            FastMode = false,
            TestMode = false,
        };

        private static readonly SmsApiConfiguration ConfigWithUnregisteredSender = new(Token, Guid.NewGuid().ToString())
        {
            FastMode = false,
            TestMode = false,
        };

        private readonly SmsApiClient client;
        private readonly SmsApiClient clientWithUnregisteredSender;

        public SmsSenderClientTests()
        {
            client = CreateSmsApiClient(Config);

            clientWithUnregisteredSender = CreateSmsApiClient(ConfigWithUnregisteredSender);
        }

        [SmsApiFact]
        public async Task Sends_sms_correctly()
        {
            await client.SendAsync(Message, PhoneNumber);
        }

        [SmsApiFact]
        public async Task Gets_correct_errors_when_sender_is_unregistered_in_the_API()
        {
            await Assert.ThrowsAsync<ActionException>(() =>
                clientWithUnregisteredSender.SendAsync(Message, PhoneNumber));
        }

        private static SmsApiClient CreateSmsApiClient(SmsApiConfiguration config)
        {
            HttpClient client = new();

            SmsApiClient.ConfigureHttpClient(config, client);

            return new(config, client);
        }

        public class SmsApiFactAttribute : FactAttribute
        {
            public SmsApiFactAttribute()
            {
                if (string.IsNullOrEmpty(Token))
                {
                    Skip = "OAuth token not set";
                }
                else if (string.IsNullOrEmpty(PhoneNumber))
                {
                    Skip = "No recipient provided";
                }
            }
        }
    }
}
