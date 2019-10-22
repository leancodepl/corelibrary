using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.SmsSender.Exceptions;

namespace LeanCode.SmsSender
{
    public class SmsApiClient : ISmsSender
    {
        public const string ApiBase = "https://api.smsapi.pl";

        private static readonly ImmutableHashSet<int> ClientErrors = ImmutableHashSet.Create(
            101,  /* Invalid or no authorization data */
            102,  /* Invalid login or password */
            103,  /* Shortage of points for this user */
            105,  /* Invalid IP address */
            110,  /* Service is not available on this account */
            1000, /* Action is available only for main user */
            1001 /* Invalid action */);

        private static readonly ImmutableHashSet<int> HostErrors = ImmutableHashSet.Create(
            8,   /* Error in request */
            201, /* Internal system error */
            666, /* Internal system error */
            999 /* Internal system error */);

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SmsApiClient>();
        private readonly SmsApiHttpClient client;
        private readonly SmsApiConfiguration smsApiConfiguration;

        public SmsApiClient(SmsApiConfiguration smsApiConfiguration, SmsApiHttpClient client)
        {
            this.smsApiConfiguration = smsApiConfiguration;
            this.client = client;
        }

        public async Task SendAsync(string message, string phoneNumber)
        {
            logger.Verbose("Sending SMS using SMS Api");

            var parameters = new Dictionary<string, string>()
            {
                ["username"] = smsApiConfiguration.Login,
                ["password"] = smsApiConfiguration.Password,
                ["from"] = smsApiConfiguration.From,
                ["format"] = "json",
                ["encoding"] = "UTF-8",
                ["message"] = message,
                ["to"] = phoneNumber,
            };

            if (smsApiConfiguration.TestMode)
            {
                parameters["test"] = "1";
            }

            if (smsApiConfiguration.FastMode)
            {
                parameters["fast"] = "1";
            }

            using (var body = new FormUrlEncodedContent(parameters))
            using (var response = await client.Client.PostAsync("sms.do", body))
            {
                await using var content = await response.Content.ReadAsStreamAsync();
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    HandleResponse(doc.RootElement);
                }
                catch (Exception e) when (e is JsonException || e is KeyNotFoundException || e is FormatException)
                {
                    throw new SerializationException("Failed to parse error message.", e);
                }
            }

            logger.Information("SMS to {PhoneNumber} sent successfully", phoneNumber);
        }

        private static void HandleResponse(JsonElement response)
        {
            if (response.TryGetProperty("error", out var error))
            {
                var errorCode = error.GetInt32();
                var errorMessage = response.GetProperty("message").GetString();

                if (IsClientError(errorCode))
                {
                    throw new ClientException(errorCode, errorMessage);
                }
                else if (IsHostError(errorCode))
                {
                    throw new HostException(errorCode, errorMessage);
                }
                else
                {
                    throw new ActionException(errorCode, errorMessage);
                }
            }
        }

        private static bool IsClientError(int code) => ClientErrors.Contains(code);

        private static bool IsHostError(int code) => HostErrors.Contains(code);
    }
}
