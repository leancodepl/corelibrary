using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class NullSession : ISession
{
    public static readonly NullSession Empty = new();

    public bool IsAvailable => false;

    public string Id => "";

    public IEnumerable<string> Keys => [ ];

    private NullSession() { }

    public void Clear() { }

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) { }

    public void Set(string key, byte[] value) { }

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
    {
        value = null;
        return false;
    }
}
