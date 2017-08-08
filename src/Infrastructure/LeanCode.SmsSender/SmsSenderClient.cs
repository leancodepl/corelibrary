using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using LeanCode.SmsSender.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeanCode.SmsSender
{
    public class SmsSenderClient : ISmsSender, IDisposable
    {
        private const string smsSenderUrl = "https://api.smsapi.pl/sms.do";

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SmsSenderClient>();
        private readonly HttpClient client;
        private readonly SmsApiConfiguration smsApiConfiguration;
        private readonly Dictionary<string, string> parameters;
        private static int[] clientErrors = {
                101,  // Invalid or no authorization data
                102,  // Invalid login or password
                103,  // Shortage of points for this user
                105,  // Invalid IP address
                110,  // Service is not available on this account
                1000, // Action is available only for main user
                1001  // Invalid action
            };
        private static int[] hostErrors = {
                8,   // Error in request
                666, // Internal system error
                999, // Internal system error
                201  // Internal system error
            };

        public SmsSenderClient(SmsApiConfiguration smsApiConfiguration)
        {
            this.smsApiConfiguration = smsApiConfiguration;

            parameters = new Dictionary<string, string>();

            parameters["username"] = smsApiConfiguration.Login;
            parameters["password"] = smsApiConfiguration.Password;
            parameters["from"] = smsApiConfiguration.From;
            parameters["format"] = "json";
            parameters["encoding"] = "UTF-8";

            if (smsApiConfiguration.TestMode)
                parameters["test"] = "1";
            if (smsApiConfiguration.FastMode)
                parameters["fast"] = "1";

            client = new HttpClient();
        }

        public SmsSenderClient Message(string message)
        {
            parameters["message"] = message;
            return this;
        }

        public SmsSenderClient To(string phoneNumber)
        {
            parameters["to"] = phoneNumber;
            return this;
        }

        public SmsSenderClient From(string sender)
        {
            parameters["from"] = sender;
            return this;
        }

        public async Task Send()
        {
            var body = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(smsSenderUrl, body);
            var content = await response.Content.ReadAsStringAsync();
            HandleResponse(content);
        }

        public async Task Send(string message, string phoneNumber)
        {
            logger.Verbose("Sending sms to {PhoneNumber} with content {Message}", phoneNumber, message);

            await this
                .To(phoneNumber)
                .Message(message)
                .Send();
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public static void HandleResponse(string response)
        {
            var parsedResponse = JObject.Parse(response);
            if (parsedResponse.Property("error") != null)
            {
                try
                {
                    Responses.Error error = JsonConvert.DeserializeObject<Responses.Error>(response);

                    if (isClientError(error.Code))
                    {
                        throw new ClientException(error.Code, error.Message);
                    }
                    if (isHostError(error.Code))
                    {
                        throw new HostException(error.Code, error.Message);
                    }
                    throw new ActionException(error.Code, error.Message);
                }
                catch (JsonSerializationException)
                {
                    throw new SerializationException();
                }
            }
        }

        private static bool isClientError(int code)
        {
            return clientErrors.Contains(code);
        }

        private static bool isHostError(int code)
        {
            return hostErrors.Contains(code);
        }
    }

}