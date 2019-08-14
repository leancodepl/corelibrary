using System.Collections.Generic;
using LeanCode.EmailSender.Model;

namespace LeanCode.EmailSender.SendGrid
{
    internal class SendGridEmail
    {
        public string Email { get; set; }
        public string Name { get; set; }

        public SendGridEmail(EmailAddress emailAddress)
        {
            Email = emailAddress.Email;
            Name = emailAddress.Name;
        }
    }

    internal class SendGridPersonalization
    {
        public List<SendGridEmail> To { get; set; }
    }

    internal class SendGridContent
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    internal class SendGridMessage
    {
        public List<SendGridPersonalization> Personalizations { get; set; }
        public SendGridEmail From { get; set; }
        public SendGridContent[] Content { get; set; }
        public SendGridAttachment[] Attachments { get; set; }
        public string Subject { get; set; }

        public bool ShouldSerializeAttachments()
        {
            return Attachments != null && Attachments.Length > 0;
        }
    }

    internal class SendGridAttachment
    {
        public string Content { get; set; }
        public string Type { get; set; }
        public string Filename { get; set; }
    }
}
