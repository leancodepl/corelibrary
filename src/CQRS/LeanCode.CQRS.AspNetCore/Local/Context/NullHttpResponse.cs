using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class NullHttpResponse : HttpResponse
{
    public override HttpContext HttpContext { get; }
    public override IHeaderDictionary Headers => NullHeaderDictionary.Empty;
    public override IResponseCookies Cookies => NullResponseCookies.Empty;

    public override bool HasStarted => false;

    public override long? ContentLength
    {
        get => null;
        set { }
    }

    public override int StatusCode
    {
        get => 0;
        set { }
    }

    public override string? ContentType
    {
        get => null;
        set { }
    }

    public override Stream Body
    {
        get => Stream.Null;
        set { }
    }

    public NullHttpResponse(HttpContext context)
    {
        HttpContext = context;
    }

    public override void OnCompleted(Func<object, Task> callback, object state) { }

    public override void Redirect(string location, bool permanent) { }

    public override void OnStarting(Func<object, Task> callback, object state) { }
}
