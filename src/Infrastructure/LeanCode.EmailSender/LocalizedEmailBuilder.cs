using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;
using LeanCode.Localization.StringLocalizers;
using static System.Globalization.CultureInfo;

namespace LeanCode.EmailSender
{
    public class LocalizedEmailBuilder
    {
        private readonly CultureInfo culture;
        private readonly IStringLocalizer stringLocalizer;
        private readonly EmailBuilder inner;

        public string? Subject => inner.Subject;
        public EmailAddress? FromEmail => inner.FromEmail;
        public IReadOnlyCollection<EmailAddress> Recipients => inner.Recipients;
        public IReadOnlyCollection<EmailContent> Contents => inner.Contents;
        public IReadOnlyCollection<EmailAttachment> Attachments => inner.Attachments;

        public LocalizedEmailBuilder(
            string cultureName,
            IStringLocalizer stringLocalizer,
            IEmailClient emailClient)
        {
            this.stringLocalizer = stringLocalizer;
            this.culture = GetCultureInfo(cultureName);
            this.inner = new EmailBuilder(emailClient);
        }

        public LocalizedEmailBuilder From(string email, string? name)
        {
            inner.From(email, name);

            return this;
        }

        public LocalizedEmailBuilder To(string email, string? name)
        {
            inner.To(email, name);

            return this;
        }

        public LocalizedEmailBuilder WithSubject(string subjectKey)
        {
            inner.WithSubject(stringLocalizer[culture, subjectKey]);

            return this;
        }

        public LocalizedEmailBuilder WithSubject(string subjectFormatKey, params object[] arguments)
        {
            inner.WithSubject(string.Format(culture, stringLocalizer[culture, subjectFormatKey], arguments));

            return this;
        }

        public LocalizedEmailBuilder WithHtmlContent(object model, string templateBaseName)
        {
            inner.WithHtmlContent(model, GenerateTemplateNames(templateBaseName));

            return this;
        }

        public LocalizedEmailBuilder WithTextContent(object model, string templateBaseName)
        {
            inner.WithTextContent(model, GenerateTemplateNames(templateBaseName, ".txt"));

            return this;
        }

        public LocalizedEmailBuilder WithHtmlContent(object model) =>
            WithHtmlContent(model, model.GetType().Name);

        public LocalizedEmailBuilder WithTextContent(object model) =>
            WithTextContent(model, model.GetType().Name);

        public LocalizedEmailBuilder Attach(Stream attachment, string name, string contentType)
        {
            inner.Attach(attachment, name, contentType);

            return this;
        }

        public Task SendAsync() => inner.SendAsync();

        private IEnumerable<string> GenerateTemplateNames(string templateBaseName, string suffix = "")
        {
            for (var c = culture; c != InvariantCulture; c = c.Parent)
            {
                yield return $"{templateBaseName}.{c.Name}{suffix}";
            }

            yield return $"{templateBaseName}{suffix}";
        }
    }
}
