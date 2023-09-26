using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.Firebase.FCM;

public interface IPushNotificationTokenStore
{
    public const int MaxTokenBatchSize = 100;

    Task<List<string>> GetTokensAsync(string userId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, List<string>>> GetTokensAsync(
        IReadOnlySet<string> userIds,
        CancellationToken cancellationToken = default
    );

    Task AddUserTokenAsync(string userId, string newToken, CancellationToken cancellationToken = default);
    Task RemoveUserTokenAsync(string userId, string newToken, CancellationToken cancellationToken = default);

    Task RemoveTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RemoveTokensAsync(IEnumerable<string> tokens, CancellationToken cancellationToken = default);
}
