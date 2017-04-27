using System.IO;
using System.Threading.Tasks;

namespace LeanCode.EmailSender
{
    public interface IEmailSender
    {
        IEmailSender From(string email, string name = null);
        IEmailSender To(string email, string name = null);
        IEmailSender WithSubject(string subject);
        IEmailSender WithModel<TModel>(TModel model);
        IEmailSender WithModelTxt<TModel>(TModel model);
        IEmailSender Attachment(Stream attachment, string name, string contentType);
        Task Send();
    }
}
