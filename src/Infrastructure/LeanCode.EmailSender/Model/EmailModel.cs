using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LeanCode.EmailSender.Model
{
    public class EmailModel
    {
        public string Subject { get; }
        public EmailAddress FromEmail { get; }
        public ImmutableArray<EmailAddress> Recipients { get; }
        public ImmutableArray<EmailContent> Contents { get; }
        public ImmutableArray<EmailAttachment> Attachments { get; }

        public EmailModel(
            string subject, EmailAddress from,
            IReadOnlyCollection<EmailAddress> recipients,
            IReadOnlyCollection<EmailContent> contents,
            IReadOnlyCollection<EmailAttachment> attachments)
        {
            if ((recipients ?? throw new ArgumentNullException(nameof(recipients))).Count == 0)
            {
                throw new ArgumentException("At least one recipient must be specified.");
            }

            Subject = subject;
            FromEmail = from ?? throw new ArgumentNullException(nameof(from));
            Recipients = recipients.ToImmutableArray();
            Contents = (contents ?? throw new ArgumentNullException(nameof(contents))).ToImmutableArray();
            Attachments = (attachments ?? throw new ArgumentNullException(nameof(attachments))).ToImmutableArray();
        }
    }
}
