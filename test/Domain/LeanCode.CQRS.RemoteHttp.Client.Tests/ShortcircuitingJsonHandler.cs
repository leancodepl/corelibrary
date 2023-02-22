using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.CQRS.RemoteHttp.Client.Tests;

internal class ShortcircuitingJsonHandler : HttpMessageHandler
{
    private readonly HttpStatusCode statusCode = HttpStatusCode.OK;
    private readonly string response = "";

    public HttpRequestMessage? Request { get; set; }

    public ShortcircuitingJsonHandler(HttpStatusCode statusCode, string response)
    {
        this.statusCode = statusCode;
        this.response = response;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Request = await CloneRequestAsync(request);
        return new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(response, Encoding.UTF8, "application/json"),
        };
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri) { Version = req.Version, };

        if (req.Content != null)
        {
            var ms = new MemoryStream();
            await req.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            if (req.Content.Headers != null)
            {
                foreach (var h in req.Content.Headers)
                {
                    clone.Content.Headers.Add(h.Key, h.Value);
                }
            }
        }

        foreach (var prop in req.Options)
        {
            ((IDictionary<string, object?>)clone.Options).Add(prop);
        }

        foreach (var header in req.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
