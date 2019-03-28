using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;
using LeanCode.Localization.StringLocalizers;

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
            this.culture = CultureInfo.GetCultureInfo(cultureName
                ?? throw new ArgumentNullException(nameof(cultureName)));

            this.stringLocalizer = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));

            this.inner = new EmailBuilder(
                emailClient ?? throw new ArgumentNullException(nameof(emailClient)),
                null);
        }

        public LocalizedEmailBuilder From(string email, string name)
        {
            inner.From(
                email ?? throw new ArgumentNullException(nameof(email)),
                name);

            return this;
        }

        public LocalizedEmailBuilder To(string email, string name)
        {
            inner.To(
                email ?? throw new ArgumentNullException(nameof(email)),
                name);

            return this;
        }

        public LocalizedEmailBuilder WithSubject(string subjectTerm)
        {
            inner.WithSubject(stringLocalizer[
                culture,
                subjectTerm ?? throw new ArgumentNullException(nameof(subjectTerm))]);

            return this;
        }

        public LocalizedEmailBuilder WithSubject(string subjectTerm, params object[] arguments)
        {
            inner.WithSubject(stringLocalizer[
                culture,
                subjectTerm ?? throw new ArgumentNullException(nameof(subjectTerm)),
                arguments ?? throw new ArgumentNullException(nameof(arguments))]);

            return this;
        }

        public LocalizedEmailBuilder WithHtmlContent(object model, string templateBaseName)
        {
            if (templateBaseName is null)
            {
                throw new ArgumentNullException(nameof(templateBaseName));
            }

            if (culture.Name.Length == 0) // InvariantCulture
            {
                inner.WithHtmlContent(
                    model ?? throw new ArgumentNullException(nameof(model)),
                    templateBaseName);
            }
            else
            {
                inner.WithHtmlContent(
                    model ?? throw new ArgumentNullException(nameof(model)),
                    $"{templateBaseName}.{culture.Name}",
                    templateBaseName);
            }

            return this;
        }

        public LocalizedEmailBuilder WithTextContent(object model, string templateBaseName)
        {
            if (templateBaseName is null)
            {
                throw new ArgumentNullException(nameof(templateBaseName));
            }

            if (culture.Name.Length == 0) // InvariantCulture
            {
                inner.WithTextContent(
                    model ?? throw new ArgumentNullException(nameof(model)),
                    templateBaseName + ".txt");
            }
            else
            {
                inner.WithTextContent(
                    model ?? throw new ArgumentNullException(nameof(model)),
                    $"{templateBaseName}.{culture.Name}.txt",
                    templateBaseName + ".txt");
            }

            return this;
        }

        public LocalizedEmailBuilder WithHtmlContent(object model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return WithHtmlContent(model, model.GetType().Name);
        }

        public LocalizedEmailBuilder WithTextContent(object model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return WithTextContent(model, model.GetType().Name);
        }

        public LocalizedEmailBuilder Attach(Stream attachment, string name, string contentType)
        {
            inner.Attach(
                attachment ?? throw new ArgumentNullException(nameof(attachment)),
                name ?? throw new ArgumentNullException(nameof(name)),
                contentType ?? throw new ArgumentNullException(nameof(contentType)));

            return this;
        }

        public Task Send() => inner.Send();
    }
}
