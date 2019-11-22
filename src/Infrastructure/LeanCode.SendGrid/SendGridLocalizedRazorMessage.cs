using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace LeanCode.SendGrid
{
    public class SendGridLocalizedRazorMessage : SendGridRazorMessage
    {
        [JsonIgnore]
        internal CultureInfo Culture { get; private set; }

        [JsonIgnore]
        public object[]? SubjectFormatArgs { get; set; }

        public SendGridLocalizedRazorMessage(string cultureName)
        {
            Culture = CultureInfo.GetCultureInfo(cultureName);
        }

        [Obsolete("Use SetGlobalSubject(string, object[]) overload instead.")]
        public new void SetGlobalSubject(string subject) => base.SetGlobalSubject(subject);

        public void SetGlobalSubject(string subjectKey, object[]? subjectFormatArgs)
        {
            base.SetGlobalSubject(subjectKey);

            SubjectFormatArgs = subjectFormatArgs;
        }

        internal override IEnumerable<string> GenerateTemplateNames(string templateBaseName)
        {
            for (var c = Culture; c != CultureInfo.InvariantCulture; c = c.Parent)
            {
                yield return $"{templateBaseName}.{c.Name}";
            }

            yield return templateBaseName;
        }
    }
}
