using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;
using Microsoft.Extensions.Localization;

namespace LeanCode.EmailSender
{
    public class LocalizedEmailBuilder
    {
        private readonly string cultureName;
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
            this.cultureName = cultureName ?? throw new ArgumentNullException(nameof(cultureName));

            this.stringLocalizer = (stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer)))
                .WithCulture(CultureInfo.GetCultureInfo(cultureName));

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

        public LocalizedEmailBuilder WithSubject(string subjectTerm, params object[] args)
        {
            inner.WithSubject(stringLocalizer[
                subjectTerm ?? throw new ArgumentNullException(nameof(subjectTerm)),
                args]);

            return this;
        }

        public LocalizedEmailBuilder WithHtmlContent(object model, string templateBaseName)
        {
            if (templateBaseName is null)
            {
                throw new ArgumentNullException(nameof(templateBaseName));
            }

            if (cultureName.Length == 0) // InvariantCulture
            {
                inner.WithHtmlContent(
                    model ?? throw new ArgumentNullException(nameof(model)),
                    templateBaseName);
            }
            else
            {
                inner.WithHtmlContent(
                    model ?? throw new ArgumentNullException(nameof(model)),
                    $"{templateBaseName}.{cultureName}",
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

            if (cultureName.Length == 0) // InvariantCulture
            {
                inner.WithTextContent(
                    model ?? throw new ArgumentNullException(nameof(model)),
                    templateBaseName + ".txt");
            }
            else
            {
                inner.WithTextContent(
                    model ?? throw new ArgumentNullException(nameof(model)),
                    $"{templateBaseName}.{cultureName}.txt",
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
