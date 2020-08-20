using System;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.Firebase.FCM;
using LeanCode.IntegrationTests.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.IntegrationTests
{
    public class PushNotificationsTests : IAsyncLifetime
    {
        private const string Token = "token";
        private readonly Guid userId = Guid.NewGuid();

        private readonly TestApp app;

        public PushNotificationsTests()
        {
            app = new TestApp();
        }

        [Fact]
        public async Task Adds_user_token_to_the_store_and_database_and_is_able_to_remove_it_keeping_other_tokens_intact()
        {
            await AddTokenAsync();
            await EnsureTokenIsInDatabaseAsync();
            await EnsureTokenIsInStoreAsync();
            await RemoveUserTokenAsync();
            await EnsureNoTokenIsInDatabaseAsync();
            await EnsureNoTokenIsInStoreAsync();

            var anotherUser = Guid.NewGuid();
            await AddTokenAsync(anotherUser, customToken: "another token");
            await AddTokenAsync();
            await RemoveUserTokenAsync(anotherUser);
            await EnsureTokenIsInDatabaseAsync();
            await EnsureTokenIsInStoreAsync();
        }

        [Fact]
        public async Task Adds_user_token_to_the_store_and_database_and_is_able_to_remove_it_by_just_the_token_value()
        {
            await AddTokenAsync();
            await EnsureTokenIsInDatabaseAsync();
            await EnsureTokenIsInStoreAsync();
            await RemoveTokenAsync();
            await EnsureNoTokenIsInDatabaseAsync();
            await EnsureNoTokenIsInStoreAsync();
        }

        [Fact]
        public async Task Adds_user_token_to_the_store_and_database_and_is_able_to_remove_it_using_collection_removal()
        {
            await AddTokenAsync();
            await EnsureTokenIsInDatabaseAsync();
            await EnsureTokenIsInStoreAsync();
            await RemoveAllTokensAsync();
            await EnsureNoTokenIsInDatabaseAsync();
            await EnsureNoTokenIsInStoreAsync();
        }

        [Fact]
        public async Task Token_uniqueness_violation_is_a_noop()
        {
            await AddTokenAsync();
            await AddTokenAsync();
            await EnsureTokenIsInDatabaseAsync();
            await EnsureTokenIsInStoreAsync();
        }

        [Fact]
        public async Task Changing_owner_of_the_token_is_correctly_handled()
        {
            var anotherUser = Guid.NewGuid();
            await AddTokenAsync(anotherUser);
            await AddTokenAsync();
            await EnsureTokenIsInDatabaseAsync();
            await EnsureTokenIsInStoreAsync();
            await EnsureNoTokenIsInDatabaseAsync(anotherUser);
            await EnsureNoTokenIsInStoreAsync(anotherUser);
        }

        public Task InitializeAsync() => app.InitializeAsync();
        public Task DisposeAsync() => app.DisposeAsync();

        private async Task AddTokenAsync(Guid? customUserId = null, string customToken = Token)
        {
            using var scope = app.Server.Services.CreateScope();
            var effectiveUserId = customUserId ?? userId;

            var store = scope.ServiceProvider.GetRequiredService<IPushNotificationTokenStore>();
            await store.AddUserTokenAsync(effectiveUserId, customToken);
        }

        private async Task EnsureTokenIsInDatabaseAsync(Guid? customUserId = null)
        {
            using var scope = app.Server.Services.CreateScope();
            var effectiveUserId = customUserId ?? userId;

            var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var tokens = await dbContext.PNTokens
                .Where(t => t.UserId == effectiveUserId)
                .Select(t => t.Token)
                .ToListAsync();
            var token = Assert.Single(tokens);
            Assert.Equal(Token, token);
        }

        private async Task EnsureTokenIsInStoreAsync(Guid? customUserId = null)
        {
            using var scope = app.Server.Services.CreateScope();
            var effectiveUserId = customUserId ?? userId;

            var store = scope.ServiceProvider.GetRequiredService<IPushNotificationTokenStore>();
            var tokens = await store.GetTokensAsync(effectiveUserId);
            var token = Assert.Single(tokens);
            Assert.Equal(Token, token);
        }

        private async Task RemoveUserTokenAsync(Guid? customUserId = null)
        {
            using var scope = app.Server.Services.CreateScope();
            var effectiveUserId = customUserId ?? userId;

            var store = scope.ServiceProvider.GetRequiredService<IPushNotificationTokenStore>();
            await store.RemoveUserTokenAsync(effectiveUserId, Token);
        }

        private async Task RemoveTokenAsync()
        {
            using var scope = app.Server.Services.CreateScope();

            var store = scope.ServiceProvider.GetRequiredService<IPushNotificationTokenStore>();
            await store.RemoveTokenAsync(Token);
        }

        private async Task RemoveAllTokensAsync()
        {
            using var scope = app.Server.Services.CreateScope();

            var store = scope.ServiceProvider.GetRequiredService<IPushNotificationTokenStore>();
            await store.RemoveTokensAsync(new[] { Token });
        }

        private async Task EnsureNoTokenIsInDatabaseAsync(Guid? customUserId = null)
        {
            using var scope = app.Server.Services.CreateScope();
            var effectiveUserId = customUserId ?? userId;

            var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var tokens = await dbContext.PNTokens
                .Where(t => t.UserId == effectiveUserId)
                .Select(t => t.Token)
                .ToListAsync();
            Assert.Empty(tokens);
        }

        private async Task EnsureNoTokenIsInStoreAsync(Guid? customUserId = null)
        {
            using var scope = app.Server.Services.CreateScope();
            var effectiveUserId = customUserId ?? userId;

            var store = scope.ServiceProvider.GetRequiredService<IPushNotificationTokenStore>();
            var tokens = await store.GetTokensAsync(effectiveUserId);
            Assert.Empty(tokens);
        }
    }
}
