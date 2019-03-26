namespace LeanCode.EmailSender
{
    public interface IEmailClient
    {
        EmailBuilder New();
        LocalizedEmailBuilder Localized(string cultureName);
    }
}
