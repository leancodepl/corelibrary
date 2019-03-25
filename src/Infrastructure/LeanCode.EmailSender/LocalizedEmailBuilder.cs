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
        public List<EmailAddress> Recipients => inner.Recipients;
        public List<EmailContent> Contents => inner.Contents;
        public List<EmailAttachment> Attachments => inner.Attachments;

        public LocalizedEmailBuilder(
            string cultureName,
            IStringLocalizer stringLocalizer,
            Func<EmailBuilder, Task> send)
        {
            this.cultureName = cultureName ?? throw new ArgumentNullException(nameof(cultureName));

            this.stringLocalizer = (stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer)))
                .WithCulture(CultureInfo.GetCultureInfo(cultureName));

            this.inner = new EmailBuilder(send ?? throw new ArgumentNullException(nameof(send)), null);
        }

        public LocalizedEmailBuilder From(string email, string name)
        {
            inner.From(email ?? throw new ArgumentNullException(nameof(email)), name);
            return this;
        }

        public LocalizedEmailBuilder To(string email, string name)
        {
            inner.To(email ?? throw new ArgumentNullException(nameof(email)), name);
            return this;
        }

        public LocalizedEmailBuilder WithSubject(string subjectTerm, params object[] args)
        {
            if (subjectTerm is null)
                throw new ArgumentNullException(nameof(subjectTerm));

            inner.WithSubject(stringLocalizer[subjectTerm, args]);
            return this;
        }

        public LocalizedEmailBuilder WithHtmlContent(object model, string templateBaseName)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            if (templateBaseName is null)
                throw new ArgumentNullException(nameof(templateBaseName));

            if (cultureName.Length is 0) // InvariantCulture
                inner.WithHtmlContent(model, templateBaseName);
            else
                inner.WithHtmlContent(model, $"{templateBaseName}.{cultureName}");

            return this;
        }

        public LocalizedEmailBuilder WithTextContent(object model, string templateBaseName)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            if (templateBaseName is null)
                throw new ArgumentNullException(nameof(templateBaseName));

            if (cultureName.Length is 0) // InvariantCulture
                inner.WithTextContent(model, templateBaseName + ".txt");
            else
                inner.WithTextContent(model, $"{templateBaseName}.{cultureName}.txt");

            return this;
        }

        public LocalizedEmailBuilder WithHtmlContent(object model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            return WithHtmlContent(model, model.GetType().Name);
        }

        public LocalizedEmailBuilder WithTextContent(object model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

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
