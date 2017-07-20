using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LeanCode.EmailSender.SendGrid
{
    class SendGridClient : IEmailClient
    {
        private readonly Serilog.ILogger logger
            = Serilog.Log.ForContext<SendGridClient>();

        private readonly SendGridConfiguration sendGridConfiguration;

        public EmailAddress FromEmail { get; set; }
        public List<EmailAddress> Recipients { get; set; }
        public List<EmailContent> Contents { get; set; }
        public List<EmailAttachment> Attachments { get; set; }
        public string Subject { get; set; }

        public SendGridClient(SendGridConfiguration sendGridConfiguration)
        {
            this.sendGridConfiguration = sendGridConfiguration;

            this.Recipients = new List<EmailAddress>();
            this.Contents = new List<EmailContent>();
            this.Attachments = new List<EmailAttachment>();
        }

        public async Task Send()
        {
            if (FromEmail == null)
                throw new ArgumentNullException("EmailClient failure. \"From\" email is necessary.");

            logger.Verbose("Sending e-mail to {Emails} with subject {Subject}",
                Recipients, Subject);

            var message = BuildMessage();
            var response = await GetHttpClient()
                .PostAsync("mail/send",
                    new StringContent(ConvertMessageToJson(message),
                    Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                logger.Warning("Cannot send e-mail with subject {Subject} to {Emails}", Subject, Recipients);

                string errorJson = await response.Content.ReadAsStringAsync();
                throw new Exception($"SendGrid indicated failure, code: {response.StatusCode}, reason: {errorJson}");
            }

            logger.Information("E-mail with subject {Subject} sent to {Emails}", Subject, Recipients);
        }

        private string ConvertMessageToJson(SendGridMessage message)
        {
            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        private SendGridMessage BuildMessage()
        {
            return new SendGridMessage()
            {
                From = new SendGridEmail(FromEmail),
                Personalizations = new List<SendGridPersonalization>()
                {
                    new SendGridPersonalization()
                    {
                        To = Recipients.Select(recipient => new SendGridEmail(recipient)).ToList()
                    }
                },
                Subject = Subject,
                Content = Contents.Select(content => new SendGridContent(content)).ToList(),
                Attachments = Attachments.Select(attachment => new SendGridAttachment(attachment)).ToList()
            };
        }

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sendGridConfiguration.ApiKey);
            client.BaseAddress = new Uri("https://api.sendgrid.com/v3/");
            return client;
        }
    }
}
