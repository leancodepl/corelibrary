using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class NullRequestCookieCollection : IRequestCookieCollection
{
    public static readonly NullRequestCookieCollection Empty = new();

    public string? this[string key] => null;

    public int Count => 0;

    public ICollection<string> Keys => [];

    private NullRequestCookieCollection() { }

    public bool ContainsKey(string key) => false;

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() =>
        Enumerable.Empty<KeyValuePair<string, string>>().GetEnumerator();

    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        value = null;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
