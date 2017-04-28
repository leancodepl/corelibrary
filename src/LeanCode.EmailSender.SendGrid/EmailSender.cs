using System;
using System.IO;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;
using LeanCode.ViewRenderer;

namespace LeanCode.EmailSender.SendGrid
{
    class EmailSender : IEmailSender
    {
        private readonly IViewRenderer viewRenderer;
        private readonly IEmailClient emailClient;

        public EmailSender(IViewRenderer viewRenderer, IEmailClient emailClient)
        {
            this.viewRenderer = viewRenderer;
            this.emailClient = emailClient;
        }

        public IEmailSender From(string email, string name)
        {
            emailClient.FromEmail = new EmailAddress(email, name);
            return this;
        }

        public IEmailSender To(string email, string name)
        {
            foreach (string emailAddress in email.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                emailClient.Recipients.Add(new EmailAddress(emailAddress, name));
            }
            return this;
        }

        public IEmailSender WithSubject(string subject)
        {
            emailClient.Subject = subject;
            return this;
        }

        public IEmailSender WithModel<TModel>(TModel model)
        {
            string viewName = GetViewNameFromModel<TModel>();
            string email = viewRenderer.RenderToString(viewName, model);
            emailClient.Contents.Add(new EmailContent(email, "text/html"));
            return this;
        }

        public IEmailSender WithModelTxt<TModel>(TModel model)
        {
            string viewName = GetViewNameFromModel<TModel>() + ".txt";
            string email = viewRenderer.RenderToString(viewName, model);
            emailClient.Contents.Add(new EmailContent(email, "text"));
            return this;
        }

        private string GetViewNameFromModel<TModel>()
        {
            string viewName = typeof(TModel).Name;
            return viewName.Substring(0, viewName.Length - 2);
        }

        public async Task Send()
        {
            await emailClient.Send();
        }

        public IEmailSender Attachment(Stream attachment, string name, string contentType)
        {
            attachment.Seek(0, SeekOrigin.Begin);

            using (MemoryStream ms = new MemoryStream())
            {
                attachment.CopyTo(ms);
                var attachmentBytes = ms.ToArray();
                var attachmentContent = System.Convert.ToBase64String(attachmentBytes);
                emailClient.Attachments.Add(new EmailAttachment(attachmentContent, name, contentType));
            }

            return this;
        }
    }
}