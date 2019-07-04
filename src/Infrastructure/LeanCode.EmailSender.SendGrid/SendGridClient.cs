using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;
using LeanCode.Localization.StringLocalizers;
using LeanCode.ViewRenderer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LeanCode.EmailSender.SendGrid
{
    public class SendGridClient : IEmailClient
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SendGridClient>();

        private readonly IStringLocalizer stringLocalizer;
        private readonly IViewRenderer viewRenderer;
        private readonly SendGridHttpClient client;

        public SendGridClient(
            IStringLocalizer stringLocalizer,
            IViewRenderer viewRenderer,
            SendGridHttpClient client)
        {
            this.stringLocalizer = stringLocalizer;
            this.viewRenderer = viewRenderer;
            this.client = client;
        }

        public EmailBuilder New() => new EmailBuilder(this);

        public LocalizedEmailBuilder Localized(string cultureName) =>
            new LocalizedEmailBuilder(cultureName, stringLocalizer, this);

        public async Task SendAsync(EmailModel model)
        {
            logger.Verbose("Sending with subject {Subject}", model.Subject);

            var message = await BuildMessage(model);
            var msgJson = JsonConvert.SerializeObject(message, JsonSettings);

            using (var content = new StringContent(msgJson, Encoding.UTF8, "application/json"))
            using (var response = await client.Client.PostAsync("mail/send", content).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    logger.Warning(
                        "Cannot send e-mail with subject {Subject} to {Emails}",
                        model.Subject, model.Recipients);

                    var errorJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new Exception($"SendGrid indicated failure, code: {response.StatusCode}, reason: {errorJson}");
                }
            }

            logger.Information(
                "E-mail with subject {Subject} sent to {Emails}",
                model.Subject, model.Recipients);
        }

        private async Task<SendGridMessage> BuildMessage(EmailModel model)
        {
            var attachmentsTask = Task.WhenAll(model.Attachments.Select(Convert));
            var contentsTask = Task.WhenAll(model.Contents.Select(Convert));

            await Task.WhenAll(attachmentsTask, contentsTask);

            var attachments = await attachmentsTask;
            var contents = await contentsTask;

            return new SendGridMessage()
            {
                From = new SendGridEmail(model.FromEmail),
                Personalizations = new List<SendGridPersonalization>()
                {
                    new SendGridPersonalization()
                    {
                        To = model.Recipients
                            .Select(recipient => new SendGridEmail(recipient))
                            .ToList()
                    }
                },
                Subject = model.Subject,
                Content = contents,
                Attachments = attachments
            };
        }

        private async Task<SendGridContent> Convert(EmailContent content)
        {
            var viewNames = GetViewNamesFromContent(content);

            foreach (string viewName in viewNames)
            {
                try
                {
                    return new SendGridContent
                    {
                        Type = content.ContentType,
                        Value = await viewRenderer
                            .RenderToString(viewName, content.Model)
                            .ConfigureAwait(false),
                    };
                }
                catch (ViewNotFoundException) { }
            }

            throw new ViewNotFoundException(
                null, // or string.Join(", ", viewNames)
                "Cannot locate any of matching views.");
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

        private static List<string> GetViewNamesFromContent(EmailContent content)
        {
            if (content.TemplateNames.Any(n => !string.IsNullOrWhiteSpace(n)))
            {
                return content.TemplateNames
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();
            }
            else
            {
                return new List<string>
                {
                    GetViewNameFromModel(content.Model.GetType(), content.ContentType)
                };
            }
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
