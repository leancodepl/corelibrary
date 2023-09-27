using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.Firebase.FCM.Tests;

internal sealed class StubStore : IPushNotificationTokenStore
{
    private readonly string userId;
    private readonly string token;

    public bool AllRemoved { get; private set; }
    public bool SingleRemoved { get; private set; }

    public StubStore(string userId, string token)
    {
        this.userId = userId;
        this.token = token;
    }

    public Task AddUserTokenAsync(string userId, string newToken, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveUserTokenAsync(string userId, string newToken, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (userId == this.userId)
        {
            return Task.FromResult(new List<string> { token });
        }
        else
        {
            return Task.FromResult(new List<string>());
        }
    }

    public Task<Dictionary<string, List<string>>> GetTokensAsync(
        IReadOnlySet<string> userIds,
        CancellationToken cancellationToken = default
    )
    {
        if (userIds.Contains(userId))
        {
            return Task.FromResult(new Dictionary<string, List<string>> { [userId] = new() { token } });
        }
        else
        {
            return Task.FromResult(new Dictionary<string, List<string>>());
        }
    }

    public Task RemoveTokensAsync(IEnumerable<string> tokens, CancellationToken cancellationToken = default)
    {
        if (tokens.All(t => t == token))
        {
            AllRemoved = true;
            return Task.CompletedTask;
        }
        else
        {
            throw new KeyNotFoundException();
        }
    }

    public Task RemoveTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (token == this.token)
        {
            SingleRemoved = true;
            return Task.CompletedTask;
        }
        else
        {
            throw new KeyNotFoundException();
        }
    }
}
