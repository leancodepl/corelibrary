using System;
using System.Threading.Tasks;
using LeanCode.EmailSender.Model;

namespace LeanCode.EmailSender
{
    public interface IEmailClient
    {
        [Obsolete("Use `SendAsync(EmailModel)` instead.")]
        Task Send(EmailModel email);

        EmailBuilder New();
        LocalizedEmailBuilder Localized(string cultureName);
        Task SendAsync(EmailModel email);
    }
}
