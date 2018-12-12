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
        private readonly Func<EmailBuilder, Task> send;

        public string Subject { get; private set; }
        public EmailAddress FromEmail { get; private set; }
        public List<EmailAddress> Recipients => recipients;
        public List<EmailContent> Contents => contents;
        public List<EmailAttachment> Attachments => attachments;

        public EmailBuilder(Func<EmailBuilder, Task> send)
        {
            this.send = send ?? throw new ArgumentNullException(nameof(send));
        }

        public EmailBuilder From(string email, string name = null)
        {
            email = email ?? throw new ArgumentNullException(email);
            FromEmail = new EmailAddress(email, name);
            return this;
        }

        public EmailBuilder To(string email, string name = null)
        {
            email = email ?? throw new ArgumentNullException(email);
            var split = email.Split(EmailSeparators, StringSplitOptions.RemoveEmptyEntries);
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

        public EmailBuilder WithHtmlContent(object model, string templateName = null)
        {
            model = model ?? throw new ArgumentNullException(nameof(model));

            contents.Add(new EmailContent(model, "text/html", templateName));
            return this;
        }

        public EmailBuilder WithTextContent(object model, string templateName = null)
        {
            model = model ?? throw new ArgumentNullException(nameof(model));

            contents.Add(new EmailContent(model, "text/plain", templateName));
            return this;
        }

        public EmailBuilder Attach(Stream attachment, string name, string contentType)
        {
            attachment = attachment ?? throw new ArgumentNullException(nameof(attachment));
            name = name ?? throw new ArgumentNullException(nameof(name));
            contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));

            attachments.Add(new EmailAttachment(attachment, name, contentType));
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
            return send(this);
        }
    }
}
