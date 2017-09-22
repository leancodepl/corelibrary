using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;
using LeanCode.ViewRenderer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LeanCode.EmailSender.SendGrid
{
    public class SendGridClient : IEmailClient, IDisposable
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SendGridClient>();

        private readonly IViewRenderer viewRenderer;
        private readonly HttpClient client;

        public SendGridClient(IViewRenderer viewRenderer, SendGridConfiguration config)
        {
            this.viewRenderer = viewRenderer;

            client = new HttpClient
            {
                BaseAddress = new Uri("https://api.sendgrid.com/v3/")
            };
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", config.ApiKey);
        }

        public EmailBuilder New()
        {
            return new EmailBuilder(Send);
        }

        private async Task Send(EmailBuilder builder)
        {
            logger.Verbose(
                "Sending e-mail to {Emails} with subject {Subject}",
                builder.Recipients, builder.Subject);

            var message = await BuildMessage(builder);
            var msgJson = JsonConvert.SerializeObject(message, JsonSettings);

            HttpResponseMessage response;
            using (var content = new StringContent(msgJson, Encoding.UTF8, "application/json"))
            {
                response = await client.PostAsync("mail/send", content).ConfigureAwait(false);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.Warning(
                    "Cannot send e-mail with subject {Subject} to {Emails}",
                    builder.Subject, builder.Recipients);

                var errorJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"SendGrid indicated failure, code: {response.StatusCode}, reason: {errorJson}");
            }

            logger.Information(
                "E-mail with subject {Subject} sent to {Emails}",
                builder.Subject, builder.Recipients);
        }

        public void Dispose()
        {
            client.Dispose();
        }

        private async Task<SendGridMessage> BuildMessage(EmailBuilder builder)
        {
            var attachmentsTask = Task.WhenAll(builder.Attachments.Select(Convert));
            var contentsTask = Task.WhenAll(builder.Contents.Select(Convert));

            await Task.WhenAll(attachmentsTask, contentsTask);

            var attachments = await attachmentsTask;
            var contents = await contentsTask;

            return new SendGridMessage()
            {
                From = new SendGridEmail(builder.FromEmail),
                Personalizations = new List<SendGridPersonalization>()
                {
                    new SendGridPersonalization()
                    {
                        To = builder.Recipients
                            .Select(recipient => new SendGridEmail(recipient))
                            .ToList()
                    }
                },
                Subject = builder.Subject,
                Content = contents,
                Attachments = attachments
            };
        }

        private async Task<SendGridContent> Convert(EmailContent content)
        {
            var viewName = GetViewNameFromContent(content);
            var rendered = await viewRenderer.RenderToString(viewName, content.Model).ConfigureAwait(false);
            return new SendGridContent
            {
                Type = content.ContentType,
                Value = rendered
            };
        }

        private static async Task<SendGridAttachment> Convert(EmailAttachment attachment)
        {
            var cnt = attachment.Content;
            if (cnt.CanSeek)
            {
                cnt.Seek(0, SeekOrigin.Begin);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                await cnt.CopyToAsync(ms).ConfigureAwait(false);
                var attachmentBytes = ms.ToArray();
                var attachmentContent = System.Convert.ToBase64String(attachmentBytes);
                return new SendGridAttachment
                {
                    Content = attachmentContent,
                    Filename = attachment.Name,
                    Type = attachment.ContentType
                };
            }
        }

        private static string GetViewNameFromContent(EmailContent content)
        {
            if (!string.IsNullOrWhiteSpace(content.TemplateName))
                return content.TemplateName;
            return GetViewNameFromModel(content.Model.GetType(), content.ContentType);
        }

        private static string GetViewNameFromModel(Type type, string mimeType)
        {
            var ext = mimeType == "text/plain" ? ".txt" : "";

            var viewName = type.Name;
            if (viewName.EndsWith("VM"))
            {
                return viewName.Substring(0, viewName.Length - 2) + ext;
            }
            else
            {
                return viewName + ext;
            }
        }
    }
}
