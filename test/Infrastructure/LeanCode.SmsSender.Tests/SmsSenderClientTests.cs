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
        private static readonly string Token = string.Empty;
        private static readonly string PhoneNumber = string.Empty;
        private static readonly string Message = "SmsSender works fine";

        private static readonly SmsApiConfiguration Config = new(Token, string.Empty)
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
            client = new SmsApiClient(
                Config,
                new HttpClient
                {
                    BaseAddress = new Uri(SmsApiClient.ApiBase),
                });

            clientWithUnregisteredSender = new SmsApiClient(
                ConfigWithUnregisteredSender,
                new HttpClient
                {
                    BaseAddress = new Uri(SmsApiClient.ApiBase),
                });
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
            await Assert.ThrowsAsync<ActionException>(() =>
                clientWithUnregisteredSender.SendAsync(Message, PhoneNumber));
        }
    }
}
