using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.AspNetCore.Local;

internal class LocalCallIdentifier : IHttpRequestIdentifierFeature
{
    public string TraceIdentifier { get; set; }

    public LocalCallIdentifier(string traceIdentifier)
    {
        TraceIdentifier = traceIdentifier;
    }
}
