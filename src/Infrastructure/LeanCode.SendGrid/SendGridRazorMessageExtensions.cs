using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SendGrid.Helpers.Mail;

namespace LeanCode.SendGrid
{
    public static class SendGridRazorMessageExtensions
    {
        public static SendGridRazorMessage WithSender(this SendGridRazorMessage message, EmailAddress emailAddress)
        {
            message.SetFrom(emailAddress);

            return message;
        }

        public static SendGridRazorMessage WithSender(this SendGridRazorMessage message, string email, string? name)
        {
            message.SetFrom(email, name);

            return message;
        }

        public static SendGridRazorMessage WithSender(this SendGridRazorMessage message, string email)
        {
            message.SetFrom(email, null);

            return message;
        }

        public static SendGridRazorMessage WithRecipient(this SendGridRazorMessage message, EmailAddress emailAddress)
        {
            message.AddTo(emailAddress);

            return message;
        }

        public static SendGridRazorMessage WithRecipient(this SendGridRazorMessage message, string email, string? name)
        {
            message.AddTo(email, name);

            return message;
        }

        public static SendGridRazorMessage WithRecipient(this SendGridRazorMessage message, string email)
        {
            message.AddTo(email, null);

            return message;
        }

        public static SendGridRazorMessage WithRecipients(
            this SendGridRazorMessage message,
            List<EmailAddress> emailAddresses)
        {
            message.AddTos(emailAddresses);

            return message;
        }

        public static SendGridRazorMessage WithCarbonCopyRecipient(
            this SendGridRazorMessage message,
            EmailAddress emailAddress)
        {
            message.AddCc(emailAddress);

            return message;
        }

        public static SendGridRazorMessage WithCarbonCopyRecipient(
            this SendGridRazorMessage message,
            string email,
            string? name)
        {
            message.AddCc(email, name);

            return message;
        }

        public static SendGridRazorMessage WithCarbonCopyRecipient(
            this SendGridRazorMessage message,
            string email)
        {
            message.AddCc(email, null);

            return message;
        }

        public static SendGridRazorMessage WithCarbonCopyRecipients(
            this SendGridRazorMessage message,
            List<EmailAddress> emailAddresses)
        {
            message.AddCcs(emailAddresses);

            return message;
        }

        public static SendGridRazorMessage WithBlindCarbonCopyRecipient(
            this SendGridRazorMessage message,
            EmailAddress emailAddress)
        {
            message.AddBcc(emailAddress);

            return message;
        }

        public static SendGridRazorMessage WithBlindCarbonCopyRecipient(
            this SendGridRazorMessage message,
            string email,
            string? name)
        {
            message.AddBcc(email, name);

            return message;
        }

        public static SendGridRazorMessage WithBlindCarbonCopyRecipient(
            this SendGridRazorMessage message,
            string email)
        {
            message.AddBcc(email, null);

            return message;
        }

        public static SendGridRazorMessage WithBlindCarbonCopyRecipients(
            this SendGridRazorMessage message,
            List<EmailAddress> emailAddresses)
        {
            message.AddBccs(emailAddresses);

            return message;
        }

        public static SendGridRazorMessage WithSubject(this SendGridRazorMessage message, string subject)
        {
            if (message is SendGridLocalizedRazorMessage localized)
            {
                localized.SetGlobalSubject(subject, null);
            }
            else
            {
                message.SetGlobalSubject(subject);
            }

            return message;
        }

        public static SendGridRazorMessage WithSubject(
            this SendGridRazorMessage message,
            string subject,
            params object[] formatArgs)
        {
            if (message is SendGridLocalizedRazorMessage localized)
            {
                localized.SetGlobalSubject(subject, formatArgs);
            }
            else
            {
                message.SetGlobalSubject(string.Format(subject, formatArgs));
            }

            return message;
        }

        public static SendGridRazorMessage WithPlainTextContent(this SendGridRazorMessage message, object model)
        {
            message.PlainTextContentModel = model;

            return message;
        }

        public static SendGridRazorMessage WithHtmlContent(this SendGridRazorMessage message, object model)
        {
            message.HtmlContentModel = model;

            return message;
        }

        public static SendGridRazorMessage WithAttachment(this SendGridRazorMessage message, Attachment attachment)
        {
            message.AddAttachment(attachment);

            return message;
        }

        public static SendGridRazorMessage WithAttachment(
            this SendGridRazorMessage message, string base64content, string fileName, string? mimeType)
        {
            message.AddAttachment(fileName, base64content, mimeType);

            return message;
        }

        public static async Task<SendGridRazorMessage> WithAttachmentAsync(
            this SendGridRazorMessage message, Stream content, string fileName, string? mimeType)
        {
            await message.AddAttachmentAsync(fileName, content, mimeType);

            return message;
        }

        public static async Task<SendGridRazorMessage> WithAttachmentAsync(
            this Task<SendGridRazorMessage> message, Stream content, string fileName, string? mimeType)
        {
            return await WithAttachmentAsync(await message, content, fileName, mimeType);
        }

        public static SendGridRazorMessage WithNoTracking(this SendGridRazorMessage message)
        {
            message.TrackingSettings = new TrackingSettings
            {
                ClickTracking = new ClickTracking
                {
                    Enable = false,
                },
                Ganalytics = new Ganalytics
                {
                    Enable = false,
                },
                OpenTracking = new OpenTracking
                {
                    Enable = false,
                },
                SubscriptionTracking = new SubscriptionTracking
                {
                    Enable = false,
                },
            };

            return message;
        }

        public static SendGridRazorMessage WithDelayUntil(this SendGridRazorMessage message, DateTimeOffset sendAt)
        {
            message.SendAt = sendAt.ToUnixTimeSeconds();

            return message;
        }
    }
}
