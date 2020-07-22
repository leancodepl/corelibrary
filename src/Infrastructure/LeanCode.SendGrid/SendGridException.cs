using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;

namespace LeanCode.SendGrid
{
    public class SendGridException : Exception
    {
        public HttpStatusCode ResponseStatusCode { get; }
        public ImmutableArray<string> ErrorMessages { get; } = ImmutableArray<string>.Empty;

        internal SendGridException(
            HttpStatusCode responseStatusCode,
            IEnumerable<SendGridError>? errors)
            : base($"SendGrid request failed with status code {responseStatusCode}.")
        {
            ResponseStatusCode = responseStatusCode;

            if (errors is object)
            {
                ErrorMessages = errors
                    .Select(e => e.Message!)
                    .Where(m => m is object)
                    .ToImmutableArray();
            }
        }
    }
}
