using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        public string Subject => inner.Subject;
        public EmailAddress FromEmail => inner.FromEmail;
        public IReadOnlyCollection<EmailAddress> Recipients => inner.Recipients;
        public IReadOnlyCollection<EmailContent> Contents => inner.Contents;
        public IReadOnlyCollection<EmailAttachment> Attachments => inner.Attachments;

        public LocalizedEmailBuilder(
            string cultureName,
            IStringLocalizer stringLocalizer,
            IEmailClient emailClient)
        {
            _ = cultureName ?? throw new ArgumentNullException(nameof(cultureName));
            _ = emailClient ?? throw new ArgumentNullException(nameof(emailClient));

            this.stringLocalizer = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
            this.culture = GetCultureInfo(cultureName);
            this.inner = new EmailBuilder(emailClient, null);
        }

        public LocalizedEmailBuilder From(string email, string name)
        {
            _ = email ?? throw new ArgumentNullException(nameof(email));

            inner.From(email, name);

            return this;
        }

        public LocalizedEmailBuilder To(string email, string name)
        {
            _ = email ?? throw new ArgumentNullException(nameof(email));

            inner.To(email, name);

            return this;
        }

        public LocalizedEmailBuilder WithSubject(string subjectTerm)
        {
            _ = subjectTerm ?? throw new ArgumentNullException(nameof(subjectTerm));

            inner.WithSubject(stringLocalizer[culture, subjectTerm]);

            return this;
        }

        public LocalizedEmailBuilder WithSubject(string subjectTerm, params object[] arguments)
        {
            _ = subjectTerm ?? throw new ArgumentNullException(nameof(subjectTerm));

            string format = stringLocalizer[culture, subjectTerm];
            inner.WithSubject(string.Format(culture, format, arguments));

            return this;
        }

        public LocalizedEmailBuilder WithHtmlContent(object model, string templateBaseName)
        {
            _ = model ?? throw new ArgumentNullException(nameof(model));
            _ = templateBaseName ?? throw new ArgumentNullException(nameof(templateBaseName));

            inner.WithHtmlContent(model, GenerateTemplateNames(templateBaseName));

            return this;
        }

        public LocalizedEmailBuilder WithTextContent(object model, string templateBaseName)
        {
            _ = model ?? throw new ArgumentNullException(nameof(model));
            _ = templateBaseName ?? throw new ArgumentNullException(nameof(templateBaseName));

            inner.WithTextContent(model, GenerateTemplateNames(templateBaseName, ".txt"));

            return this;
        }

        public LocalizedEmailBuilder WithHtmlContent(object model)
        {
            _ = model ?? throw new ArgumentNullException(nameof(model));

            return WithHtmlContent(model, model.GetType().Name);
        }

        public LocalizedEmailBuilder WithTextContent(object model)
        {
            _ = model ?? throw new ArgumentNullException(nameof(model));

            return WithTextContent(model, model.GetType().Name);
        }

        public LocalizedEmailBuilder Attach(Stream attachment, string name, string contentType)
        {
            _ = attachment ?? throw new ArgumentNullException(nameof(attachment));
            _ = name ?? throw new ArgumentNullException(nameof(name));
            _ = contentType ?? throw new ArgumentNullException(nameof(contentType));

            inner.Attach(attachment, name, contentType);

            return this;
        }

        public Task Send() => inner.Send();

        private IEnumerable<string> GenerateTemplateNames(
            string templateBaseName, string suffix = "")
        {
            for (var c = culture; c != InvariantCulture; c = c.Parent)
            {
                yield return $"{templateBaseName}.{c.Name}{suffix}";
            }

            yield return $"{templateBaseName}{suffix}";
        }
    }
}
