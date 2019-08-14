using System;
using System.Runtime.Serialization;

namespace LeanCode.Localization
{
    [Serializable]
    public class LocalizedResourceNotFoundException : Exception
    {
        public LocalizedResourceNotFoundException(Exception inner)
            : base(inner.Message, inner) { }

        protected LocalizedResourceNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }
    }
}
