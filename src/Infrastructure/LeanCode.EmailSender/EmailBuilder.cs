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

        public string? Subject { get; private set; } = null;
        public EmailAddress? FromEmail { get; private set; } = null;
        public IReadOnlyCollection<EmailAddress> Recipients => recipients.AsReadOnly();
        public IReadOnlyCollection<EmailContent> Contents => contents.AsReadOnly();
        public IReadOnlyCollection<EmailAttachment> Attachments => attachments.AsReadOnly();

        public EmailBuilder(IEmailClient emailClient)
        {
            this.emailClient = emailClient;
        }

        public EmailBuilder From(string email, string? name)
        {
            FromEmail = new EmailAddress(email, name);

            return this;
        }

        public EmailBuilder To(string email, string? name)
        {
            var split = email.Split(EmailSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var emailAddress in split)
            {
                recipients.Add(new EmailAddress(emailAddress, name));
            }

            return this;
        }

        public EmailBuilder WithSubject(string subject)
        {
            Subject = subject;

            return this;
        }

        internal EmailBuilder WithHtmlContent(object model, IEnumerable<string> templateNames)
        {
            contents.Add(new EmailContent(model, "text/html", templateNames));

            return this;
        }

        internal EmailBuilder WithTextContent(object model, IEnumerable<string> templateNames)
        {
            contents.Add(new EmailContent(model, "text/plain", templateNames));

            return this;
        }

        public EmailBuilder WithHtmlContent(object model, string templateName)
        {
            contents.Add(new EmailContent(model, "text/html", templateName));

            return this;
        }

        public EmailBuilder WithTextContent(object model, string templateName)
        {
            contents.Add(new EmailContent(model, "text/plain", templateName));

            return this;
        }

        public EmailBuilder WithHtmlContent(object model) =>
            WithHtmlContent(model, model.GetType().Name);

        public EmailBuilder WithTextContent(object model) =>
            WithTextContent(model, model.GetType().Name + ".txt");

        public EmailBuilder Attach(Stream attachment, string name, string contentType)
        {
            attachments.Add(new EmailAttachment(attachment, name, contentType));

            return this;
        }

        public Task SendAsync()
        {
            _ = FromEmail ?? throw new InvalidOperationException("'From' e-mail has to be specified before sending.");

            if (recipients.Count == 0)
            {
                throw new InvalidOperationException("At least one recipient has to be specified before sending.");
            }

            return emailClient.SendAsync(new EmailModel(
                Subject, FromEmail, Recipients, Contents, Attachments));
        }
    }
}
