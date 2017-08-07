using System.Threading.Tasks;
using LeanCode.SmsSender.SmsApi.Api;
using Microsoft.Extensions.Options;

namespace LeanCode.SmsSender.SmsApi
{
    public class SmsApiSender : ISmsSender
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SmsApiSender>();
        private readonly  SmsApiConfiguration smsApiConfiguration;

        public SmsApiSender(IOptions<SmsApiConfiguration> smsApiConfiguration)
        {
            this.smsApiConfiguration = smsApiConfiguration.Value;     
        }

        public async Task Send(string message, string phoneNumber)
        {
            logger.Verbose("Sending sms to {PhoneNumber} with content {Message}", phoneNumber, message);

            SmsApiClient client = new SmsApiClient(smsApiConfiguration);

            await client
                .To(phoneNumber)
                .Message(message)
                .Send();
        }
    }
}
