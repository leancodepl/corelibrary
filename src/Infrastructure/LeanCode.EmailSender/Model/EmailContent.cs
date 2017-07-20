namespace LeanCode.EmailSender.Model
{
    public class EmailContent
    {
        public string Type { get; set; }
        public string Content { get; set; }

        public EmailContent(string content, string type)
        {
            Content = content;
            Type = type;
        }
    }
}
