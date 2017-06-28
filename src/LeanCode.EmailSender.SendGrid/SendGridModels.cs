using System.Collections.Generic;
using LeanCode.EmailSender.Model;

namespace LeanCode.EmailSender.SendGrid
{
    class SendGridEmail
    {
        public string Email { get; set; }
        public string Name { get; set; }

        public SendGridEmail()
        { }

        public SendGridEmail(EmailAddress emailAddress)
        {
            Email = emailAddress.Email;
            Name = emailAddress.Name;
        }
    }

    class SendGridPersonalization
    {
        public List<SendGridEmail> To { get; set; }

        public SendGridPersonalization()
        { }
    }

    class SendGridContent
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public SendGridContent()
        { }

        public SendGridContent(EmailContent emailContent)
        {
            this.Type = emailContent.Type;
            this.Value = emailContent.Content;
        }
    }

    class SendGridMessage
    {
        public List<SendGridPersonalization> Personalizations { get; set; }
        public SendGridEmail From { get; set; }
        public List<SendGridContent> Content { get; set; }
        public List<SendGridAttachment> Attachments { get; set; }
        public string Subject { get; set; }

        public bool ShouldSerializeAttachments()
        {
            return Attachments != null && Attachments.Count > 0;
        }

        public SendGridMessage()
        { }
    }

    class SendGridAttachment
    {
        public string Content { get; set; }
        public string Type { get; set; }
        public string Filename { get; set; }

        public SendGridAttachment(EmailAttachment emailAttachment)
        {
            Content = emailAttachment.Content;
            Type = emailAttachment.ContentType;
            Filename = emailAttachment.Name;
        }
    }
}
