using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;

namespace LeanCode.EmailSender
{
    public class EmailBuilder
    {
        private static readonly char[] EmailSeparators = { ',', ';', ' ' };
        private readonly List<EmailAddress> recipients = new List<EmailAddress>();
        private readonly List<EmailContent> contents = new List<EmailContent>();
        private readonly List<EmailAttachment> attachments = new List<EmailAttachment>();
        private readonly IEmailClient emailClient;
        private readonly Action<string> logWarning;

        public string Subject { get; private set; } = null;
        public EmailAddress FromEmail { get; private set; } = null;
        public IReadOnlyCollection<EmailAddress> Recipients => recipients.AsReadOnly();
        public IReadOnlyCollection<EmailContent> Contents => contents.AsReadOnly();
        public IReadOnlyCollection<EmailAttachment> Attachments => attachments.AsReadOnly();

        public EmailBuilder(IEmailClient emailClient, Action<string> logWarning)
        {
            this.emailClient = emailClient ?? throw new ArgumentNullException(nameof(emailClient));
            this.logWarning = logWarning;
        }

        public EmailBuilder From(string email, string name)
        {
            FromEmail = new EmailAddress(
                email ?? throw new ArgumentNullException(email),
                name);

            return this;
        }

        public EmailBuilder To(string email, string name)
        {
            var split = (email ?? throw new ArgumentNullException(email))
                .Split(EmailSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var emailAddress in split)
            {
                recipients.Add(new EmailAddress(emailAddress, name));
            }

            return this;
        }

        public EmailBuilder WithSubject(string subject)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            return this;
        }

        public EmailBuilder WithSubject(string subject, params object[] args)
        {
            Subject = string.Format(
                subject ?? throw new ArgumentNullException(nameof(subject)),
                args);

            return this;
        }

        internal EmailBuilder WithHtmlContent(object model, params string[] templateNames)
        {
            contents.Add(new EmailContent(
                model ?? throw new ArgumentNullException(nameof(model)),
                "text/html",
                templateNames));

            return this;
        }

        internal EmailBuilder WithTextContent(object model, params string[] templateNames)
        {
            contents.Add(new EmailContent(
                model ?? throw new ArgumentNullException(nameof(model)),
                "text/plain",
                templateNames));

            return this;
        }

        public EmailBuilder WithHtmlContent(object model, string templateName)
        {
            if (templateName is null)
            {
                logWarning?.Invoke(
                    "Passing `null` as `templateName` is deprecated, use `WithHtmlContent(object)` overload instead");
            }

            contents.Add(new EmailContent(
                model ?? throw new ArgumentNullException(nameof(model)),
                "text/html",
                templateName));

            return this;
        }

        public EmailBuilder WithTextContent(object model, string templateName)
        {
            if (templateName is null)
            {
                logWarning?.Invoke(
                    "Passing `null` as `templateName` is deprecated, use `WithTextContent(object)` overload instead");
            }

            contents.Add(new EmailContent(
                model ?? throw new ArgumentNullException(nameof(model)),
                "text/plain",
                templateName));

            return this;
        }

        public EmailBuilder WithHtmlContent(object model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return WithHtmlContent(model, model.GetType().Name);
        }

        public EmailBuilder WithTextContent(object model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return WithTextContent(model, model.GetType().Name + ".txt");
        }

        public EmailBuilder Attach(Stream attachment, string name, string contentType)
        {
            attachments.Add(new EmailAttachment(
                attachment ?? throw new ArgumentNullException(nameof(attachment)),
                name ?? throw new ArgumentNullException(nameof(name)),
                contentType ?? throw new ArgumentNullException(nameof(contentType))));

            return this;
        }

        public Task Send()
        {
            if (FromEmail is null)
            {
                throw new InvalidOperationException("'From' e-mail has to be specified before sending.");
            }

            if (recipients.Count == 0)
            {
                throw new InvalidOperationException("At least one recipient has to be specified before sending.");
            }

            return emailClient.Send(new EmailModel(
                Subject, FromEmail, Recipients, Contents, Attachments));
        }
    }
}
