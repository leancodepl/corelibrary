namespace LeanCode.EmailSender.Model
{
    public class EmailContent
    {
        public string ContentType { get; }
        public object Model { get; }
        public string TemplateName { get; }

        public EmailContent(object model, string mimeType, string templateName)
        {
            ContentType = mimeType;
            Model = model;
            TemplateName = templateName;
        }
    }
}
