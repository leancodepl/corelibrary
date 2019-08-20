using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace LeanCode.Correlation
{
    public class HttpCorrelation
    {
        public const string Header = "X-LeanCode-CorrelationId";

        public static Guid Parse(HttpContext httpContext)
        {
            if (httpContext.Request.Headers.TryGetValue(Header, out var vals))
            {
                foreach (var v in vals)
                {
                    if (Guid.TryParse(v, out var corrId))
                    {
                        return corrId;
                    }
                }
            }
            return Guid.Empty;
        }

        public static void Populate(HttpRequestMessage msg, Guid corrId)
        {
            msg.Headers.Add(Header, corrId.ToString());
        }
    }
}
