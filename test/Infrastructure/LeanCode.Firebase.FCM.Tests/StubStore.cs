using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.Firebase.FCM.Tests
{
    internal class StubStore : IPushNotificationTokenStore
    {
        private readonly Guid userId;
        private readonly string token;

        public bool AllRemoved { get; private set; }
        public bool SingleRemoved { get; private set; }

        public StubStore(Guid userId, string token)
        {
            this.userId = userId;
            this.token = token;
        }

        public Task AddUserTokenAsync(Guid userId, string newToken, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUserTokenAsync(Guid userId, string newToken, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetTokensAsync(Guid userId, CancellationToken cancellationToken = default)
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

        public Task<Dictionary<Guid, List<string>>> GetTokensAsync(List<Guid> userIds, CancellationToken cancellationToken = default)
        {
            if (userIds.Contains(userId))
            {
                return Task.FromResult(new Dictionary<Guid, List<string>> { [userId] = new() { token } });
            }
            else
            {
                return Task.FromResult(new Dictionary<Guid, List<string>>());
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
}
