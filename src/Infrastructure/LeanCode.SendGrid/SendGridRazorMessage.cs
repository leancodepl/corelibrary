using System.Collections.Generic;
using System.Text.Json.Serialization;
using SendGrid.Helpers.Mail;

namespace LeanCode.SendGrid
{
    public class SendGridRazorMessage : SendGridMessage
    {
        [JsonIgnore]
        public object? PlainTextContentModel { get; set; }

        [JsonIgnore]
        public object? HtmlContentModel { get; set; }

        internal virtual IEnumerable<string> GenerateTemplateNames(string templateBaseName)
        {
            yield return templateBaseName;
        }
    }
}
