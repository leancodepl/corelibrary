namespace LeanCode.EmailSender
{
    public interface IEmailClient
    {
        EmailBuilder New();
        LocalizedEmailBuilder NewLocalized(string cultureName);
    }
}
