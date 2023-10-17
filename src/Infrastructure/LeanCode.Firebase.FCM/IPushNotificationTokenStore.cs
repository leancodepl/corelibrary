namespace LeanCode.Firebase.FCM;

public interface IPushNotificationTokenStore<TUserId>
    where TUserId : notnull, IEquatable<TUserId>
{
    public const int MaxTokenBatchSize = 100;

    Task<List<string>> GetTokensAsync(TUserId userId, CancellationToken cancellationToken = default);

    Task<Dictionary<TUserId, List<string>>> GetTokensAsync(
        IReadOnlySet<TUserId> userIds,
        CancellationToken cancellationToken = default
    );

    Task AddUserTokenAsync(TUserId userId, string newToken, CancellationToken cancellationToken = default);
    Task RemoveUserTokenAsync(TUserId userId, string token, CancellationToken cancellationToken = default);

    Task RemoveTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RemoveTokensAsync(IEnumerable<string> tokens, CancellationToken cancellationToken = default);
    Task RemoveAllUserTokensAsync(TUserId userId, CancellationToken cancellationToken = default);
}
