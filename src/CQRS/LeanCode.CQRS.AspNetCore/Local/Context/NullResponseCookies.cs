using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class NullResponseCookies : IResponseCookies
{
    public static readonly NullResponseCookies Empty = new();

    private NullResponseCookies() { }

    public void Append(string key, string value) { }

    public void Append(string key, string value, CookieOptions options) { }

    public void Delete(string key) { }

    public void Delete(string key, CookieOptions options) { }
}
