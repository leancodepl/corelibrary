using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using LeanCode.SmsSender.Exceptions;
using Xunit;

namespace LeanCode.SmsSender.Tests
{
    public class SmsSenderClientTests
    {
        private static readonly string Login = string.Empty;
        private static readonly string Password = string.Empty;
        private static readonly string PhoneNumber = string.Empty;
        private static readonly string Message = "SmsSender works fine";

        private static readonly SmsApiConfiguration Config = new SmsApiConfiguration
        {
            Login = Login,
            Password = Password,
            From = string.Empty,
            FastMode = false,
            TestMode = false,
        };

        private static readonly SmsApiConfiguration ConfigWithUnregisteredSender = new SmsApiConfiguration
        {
            Login = Login,
            Password = Password,
            From = Guid.NewGuid().ToString(),
            FastMode = false,
            TestMode = false,
        };

        private readonly SmsApiClient client;
        private readonly SmsApiClient clientWithUnregisteredSender;

        public SmsSenderClientTests()
        {
            client = new SmsApiClient(
                Config,
                new SmsApiHttpClient(
                    new HttpClient
                    {
                        BaseAddress = new Uri(SmsApiClient.ApiBase),
                    }));

            clientWithUnregisteredSender = new SmsApiClient(
                ConfigWithUnregisteredSender,
                new SmsApiHttpClient(
                    new HttpClient
                    {
                        BaseAddress = new Uri(SmsApiClient.ApiBase),
                    }));
        }

        [SuppressMessage("?", "xUnit1004", Justification = "Requires custom data.")]
        [Fact(Skip = "SmsApi credentials required")]
        public async Task Sends_sms_correctly()
        {
            await client.SendAsync(Message, PhoneNumber);
        }

        [SuppressMessage("?", "xUnit1004", Justification = "Requires custom data.")]
        [Fact(Skip = "SmsApi credentials required")]
        public async Task Gets_correct_errors_when_sender_is_unregistered_in_the_API()
        {
            try
            {
                await clientWithUnregisteredSender.SendAsync(Message, PhoneNumber);
            }
            catch (ClientException)
            {
            }
        }
    }
}
