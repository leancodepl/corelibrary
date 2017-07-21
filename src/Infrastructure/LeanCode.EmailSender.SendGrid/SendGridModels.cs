using System.Collections.Generic;
using LeanCode.EmailSender.Model;

namespace LeanCode.EmailSender.SendGrid
{
    class SendGridEmail
    {
        public string Email { get; set; }
        public string Name { get; set; }

        public SendGridEmail(EmailAddress emailAddress)
        {
            Email = emailAddress.Email;
            Name = emailAddress.Name;
        }
    }

    class SendGridPersonalization
    {
        public List<SendGridEmail> To { get; set; }
    }

    class SendGridContent
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    class SendGridMessage
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

    class SendGridAttachment
    {
        public string Content { get; set; }
        public string Type { get; set; }
        public string Filename { get; set; }
    }
}
