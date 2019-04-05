using System.IO;

namespace LeanCode.EmailSender.Model
{
    public class EmailAttachment
    {
        public Stream Content { get; }
        public string Name { get; }
        public string ContentType { get; }

        public EmailAttachment(Stream content, string name, string contentType)
        {
            Content = content;
            Name = name;
            ContentType = contentType;
        }
    }
}
