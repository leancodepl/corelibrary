using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LeanCode.SmsSender.SmsApi.Api
{
    public class SmsApiClient
    {
        private const string smsApiUrl = "https://api.smsapi.pl/sms.do"; 

        private readonly SmsApiConfiguration smsApiConfiguration;
        private readonly Dictionary<string, string> parameters;

        public SmsApiClient(SmsApiConfiguration smsApiConfiguration)
        {
            this.smsApiConfiguration = smsApiConfiguration;

            parameters = new Dictionary<string, string>();

            parameters["username"] = smsApiConfiguration.SmsApiLogin;
            parameters["password"] = smsApiConfiguration.SmsApiPassword;
            parameters["from"] = smsApiConfiguration.SmsApiFrom;
            parameters["format"] = "json";
            parameters["encoding"] = "UTF-8";

            if (smsApiConfiguration.TestMode)
                parameters["test"] = "1";
            if (smsApiConfiguration.FastMode)
                parameters["fast"] = "1";
        }
        
        public SmsApiClient Message(string message)
        {
            parameters["message"] = message;
            return this;
        }

        public SmsApiClient To(string phoneNumber)
        {
            parameters["to"] = phoneNumber;
            return this;
        }

        public SmsApiClient From(string sender)
        {
            parameters["from"] = sender;
            return this;
        }

        public async Task Send()
        {
            var body = new FormUrlEncodedContent(parameters);

            var response = await new HttpClient().PostAsync(smsApiUrl, body);
            var content = await response.Content.ReadAsStringAsync();

            ResponseHandler.Handle(content);
        }
    }

}