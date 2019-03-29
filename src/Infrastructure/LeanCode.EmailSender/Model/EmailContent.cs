using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LeanCode.EmailSender.Model
{
    public class EmailContent
    {
        public string ContentType { get; }
        public object Model { get; }
        public string TemplateName => TemplateNames[0];
        public ImmutableArray<string> TemplateNames { get; }

        public EmailContent(object model, string mimeType, string templateName)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            ContentType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
            TemplateNames = ImmutableArray.Create(templateName);
        }

        public EmailContent(object model, string mimeType, IEnumerable<string> templateNames)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            ContentType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
            TemplateNames = templateNames.ToImmutableArray();
        }
    }
}
