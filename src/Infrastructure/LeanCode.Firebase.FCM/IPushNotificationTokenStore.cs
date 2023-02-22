using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.Firebase.FCM;

public interface IPushNotificationTokenStore
{
    public const int MaxTokenBatchSize = 100;

    Task<List<string>> GetTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, List<string>>> GetTokensAsync(IReadOnlySet<Guid> userIds, CancellationToken cancellationToken = default);

    Task AddUserTokenAsync(Guid userId, string newToken, CancellationToken cancellationToken = default);
    Task RemoveUserTokenAsync(Guid userId, string newToken, CancellationToken cancellationToken = default);

    Task RemoveTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RemoveTokensAsync(IEnumerable<string> tokens, CancellationToken cancellationToken = default);
}
