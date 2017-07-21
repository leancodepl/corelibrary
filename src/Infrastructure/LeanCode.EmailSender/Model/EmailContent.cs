namespace LeanCode.EmailSender.Model
{
    public class EmailContent
    {
        public string ContentType { get; }
        public object Model { get; }

        public EmailContent(object model, string mimeType)
        {
            ContentType = mimeType;
            Model = model;
        }
    }
}
