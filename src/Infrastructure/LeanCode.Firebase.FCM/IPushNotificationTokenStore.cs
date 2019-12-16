using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCode.Firebase.FCM
{
    public interface IPushNotificationTokenStore
    {
        Task<List<string>> GetTokensAsync(Guid userId);
        Task AddTokenAsync(Guid userId, string newToken);
        Task RemoveTokenAsync(string token);
        Task RemoveAllTokensAsync(IEnumerable<string> tokens);
    }
}
