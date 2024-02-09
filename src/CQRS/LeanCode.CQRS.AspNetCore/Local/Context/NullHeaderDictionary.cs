using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class NullHeaderDictionary : IHeaderDictionary
{
    public static readonly NullHeaderDictionary Empty = new();

    public StringValues this[string key]
    {
        get => StringValues.Empty;
        set { }
    }

    public long? ContentLength
    {
        get => null;
        set { }
    }

    public ICollection<string> Keys { get; } = new ReadOnlyCollection<string>([ ]);

    public ICollection<StringValues> Values { get; } = new ReadOnlyCollection<StringValues>([ ]);

    public int Count => 0;

    public bool IsReadOnly => true;

    private NullHeaderDictionary() { }

    public void Add(string key, StringValues value) { }

    public void Add(KeyValuePair<string, StringValues> item) { }

    public void Clear() { }

    public bool Contains(KeyValuePair<string, StringValues> item) => false;

    public bool ContainsKey(string key) => false;

    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex) { }

    public bool Remove(string key) => false;

    public bool Remove(KeyValuePair<string, StringValues> item) => false;

    public bool TryGetValue(string key, out StringValues value)
    {
        value = StringValues.Empty;
        return false;
    }

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() =>
        Enumerable.Empty<KeyValuePair<string, StringValues>>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
