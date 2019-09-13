using System.Collections.Generic;
using System.Collections.Immutable;

namespace LeanCode.EmailSender.Model
{
    public class EmailContent
    {
        public string ContentType { get; }
        public object Model { get; }
        public ImmutableArray<string> TemplateNames { get; }

        public EmailContent(object model, string mimeType, string templateName)
        {
            Model = model;
            ContentType = mimeType;
            TemplateNames = ImmutableArray.Create(templateName);
        }

        public EmailContent(object model, string mimeType, IEnumerable<string> templateNames)
        {
            Model = model;
            ContentType = mimeType;
            TemplateNames = templateNames.ToImmutableArray();
        }
    }
}
