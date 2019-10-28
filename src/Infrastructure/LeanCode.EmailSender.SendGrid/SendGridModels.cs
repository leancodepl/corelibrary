using System.Collections.Generic;
using System.Collections.Immutable;
using LeanCode.EmailSender.Model;

namespace LeanCode.EmailSender.SendGrid
{
    internal class SendGridEmail
    {
        public string Email { get; set; }
        public string? Name { get; set; }

        public SendGridEmail(EmailAddress emailAddress)
        {
            Email = emailAddress.Email;
            Name = emailAddress.Name;
        }
    }

    internal class SendGridPersonalization
    {
        public ImmutableList<SendGridEmail> To { get; set; }

        public SendGridPersonalization(IEnumerable<SendGridEmail> to)
        {
            To = to.ToImmutableList();
        }
    }

    internal class SendGridContent
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public SendGridContent(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }

    internal class SendGridMessage
    {
        public SendGridEmail From { get; set; }
        public ImmutableList<SendGridPersonalization> Personalizations { get; set; }
        public ImmutableArray<SendGridContent> Content { get; set; }
        public ImmutableList<SendGridAttachment>? Attachments { get; set; }
        public string? Subject { get; set; }

        public SendGridMessage(SendGridEmail from, IEnumerable<SendGridPersonalization> personalizations)
        {
            From = from;
            Personalizations = personalizations.ToImmutableList();
        }
    }

    internal class SendGridAttachment
    {
        public string Content { get; set; }
        public string Type { get; set; }
        public string Filename { get; set; }

        public SendGridAttachment(string content, string type, string filename)
        {
            Content = content;
            Type = type;
            Filename = filename;
        }
    }
}
