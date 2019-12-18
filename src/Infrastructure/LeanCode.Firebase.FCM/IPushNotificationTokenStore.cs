using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCode.Firebase.FCM
{
    public interface IPushNotificationTokenStore
    {
        Task<List<string>> GetTokensAsync(Guid userId);
        Task AddUserTokenAsync(Guid userId, string newToken);
        Task RemoveUserTokenAsync(Guid userId, string newToken);

        Task RemoveTokenAsync(string token);
        Task RemoveTokensAsync(IEnumerable<string> tokens);
    }
}
