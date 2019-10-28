using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;
using LeanCode.Localization.StringLocalizers;
using LeanCode.ViewRenderer;

namespace LeanCode.EmailSender.SendGrid
{
    public class SendGridClient : IEmailClient
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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
            using var content = PrepareContent(message);
            using var response = await client.Client.PostAsync("mail/send", content);

            if (!response.IsSuccessStatusCode)
            {
                logger.Warning(
                    "Cannot send e-mail with subject {Subject} to {Emails}",
                    model.Subject, model.Recipients);

                var errorJson = await response.Content.ReadAsStringAsync();
                throw new Exception($"SendGrid indicated failure, code: {response.StatusCode}, reason: {errorJson}");
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

            return new SendGridMessage(new SendGridEmail(model.FromEmail), ImmutableList.Create(
                new SendGridPersonalization(model.Recipients
                    .Select(recipient => new SendGridEmail(recipient))
                    .ToImmutableList())))
            {
                Content = contents.ToImmutableArray(),
                Attachments = attachments.Length == 0
                    ? null
                    : attachments.ToImmutableList(),

                Subject = model.Subject,
            };
        }

        private async Task<SendGridContent> Convert(EmailContent content)
        {
            var viewNames = GetViewNamesFromContent(content);

            foreach (var viewName in viewNames)
            {
                try
                {
                    var renderedView = await viewRenderer
                        .RenderToStringAsync(viewName, content.Model);
                    return new SendGridContent(content.ContentType, renderedView);
                }
                catch (ViewNotFoundException ex)
                {
                    logger.Debug(
                        ex, "Cannot locate view {ViewName}, trying the next one",
                        viewName);
                }
            }

            throw new ViewNotFoundException(
                null, // or string.Join(", ", viewNames)
                "Cannot locate any of matching views.");
        }

        private static async Task<SendGridAttachment> Convert(EmailAttachment attachment)
        {
            var content = attachment.Content;

            if (content.CanSeek)
            {
                content.Seek(0, SeekOrigin.Begin);
            }

            using var stream = new MemoryStream();

            await content.CopyToAsync(stream);

            var attachmentBytes = stream.ToArray();
            var attachmentContent = System.Convert.ToBase64String(attachmentBytes);

            return new SendGridAttachment(attachmentContent, attachment.ContentType, attachment.Name);
        }

        private static List<string> GetViewNamesFromContent(EmailContent content) =>
            content.TemplateNames.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();

        private static ByteArrayContent PrepareContent(SendGridMessage message)
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(message, SerializerOptions);
            var content = new ByteArrayContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = Encoding.UTF8.WebName,
            };
            return content;
        }
    }
}
