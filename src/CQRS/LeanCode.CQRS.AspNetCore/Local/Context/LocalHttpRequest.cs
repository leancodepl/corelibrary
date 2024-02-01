using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class LocalHttpRequest : HttpRequest
{
    public override HttpContext HttpContext { get; }
    public override IHeaderDictionary Headers { get; }

    public override bool HasFormContentType => false;

    public override string Method
    {
        get => "";
        set { }
    }

    public override string Scheme
    {
        get => "";
        set { }
    }

    public override bool IsHttps
    {
        get => false;
        set { }
    }

    public override HostString Host
    {
        get => default;
        set { }
    }

    public override PathString PathBase
    {
        get => PathString.Empty;
        set { }
    }

    public override PathString Path
    {
        get => PathString.Empty;
        set { }
    }

    public override QueryString QueryString
    {
        get => QueryString.Empty;
        set { }
    }

    public override IQueryCollection Query
    {
        get => QueryCollection.Empty;
        set { }
    }

    public override string Protocol
    {
        get => "";
        set { }
    }

    public override IRequestCookieCollection Cookies
    {
        get => NullRequestCookieCollection.Empty;
        set { }
    }

    public override long? ContentLength
    {
        get => null;
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

    public override IFormCollection Form
    {
        get => FormCollection.Empty;
        set { }
    }

    public LocalHttpRequest(HttpContext httpContext, IHeaderDictionary? headers)
    {
        HttpContext = httpContext;
        Headers = headers ?? new HeaderDictionary();
    }

    public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IFormCollection>(FormCollection.Empty);
}
