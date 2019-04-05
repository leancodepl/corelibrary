using System.Threading.Tasks;
using LeanCode.EmailSender.Model;

namespace LeanCode.EmailSender
{
    public interface IEmailClient
    {
        EmailBuilder New();
        LocalizedEmailBuilder Localized(string cultureName);
        Task Send(EmailModel email);
    }
}
