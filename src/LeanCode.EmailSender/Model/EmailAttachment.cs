namespace LeanCode.EmailSender.Model
{
    public class EmailAttachment
    {
        public string Content { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }

        public EmailAttachment(string content, string name, string contentType)
        {
            Content = content;
            Name = name;
            ContentType = contentType;
        }
    }
}
