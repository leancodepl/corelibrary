using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;

namespace LeanCode.EmailSender
{
    public interface IEmailClient
    {
        EmailAddress FromEmail { get; set; }
        List<EmailAddress> Recipients { get; set; }
        List<EmailContent> Contents { get; set; }
        List<EmailAttachment> Attachments { get; set; }
        string Subject { get; set; }
        Task Send();
    }
}
