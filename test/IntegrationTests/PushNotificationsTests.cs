using System;
using System.Collections.Generic;
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
        public async Task Adds_user_token_to_the_store_and_database_and_is_able_to_remove_it()
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

        public Task InitializeAsync() => app.InitializeAsync();
        public Task DisposeAsync() => app.DisposeAsync();

        private async Task AddTokenAsync()
        {
            using var scope = app.Server.Services.CreateScope();

            var store = scope.ServiceProvider.GetRequiredService<IPushNotificationTokenStore>();
            await store.AddTokenAsync(userId, Token);
        }

        private async Task EnsureTokenIsInDatabaseAsync()
        {
            using var scope = app.Server.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var tokens = await dbContext.PNTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.Token)
                .ToListAsync();
            var token = Assert.Single(tokens);
            Assert.Equal(Token, token);
        }

        private async Task EnsureTokenIsInStoreAsync()
        {
            using var scope = app.Server.Services.CreateScope();

            var store = scope.ServiceProvider.GetRequiredService<IPushNotificationTokenStore>();
            var tokens = await store.GetTokensAsync(userId);
            var token = Assert.Single(tokens);
            Assert.Equal(Token, token);
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

        private async Task EnsureNoTokenIsInDatabaseAsync()
        {
            using var scope = app.Server.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var tokens = await dbContext.PNTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.Token)
                .ToListAsync();
            Assert.Empty(tokens);
        }

        private async Task EnsureNoTokenIsInStoreAsync()
        {
            using var scope = app.Server.Services.CreateScope();

            var store = scope.ServiceProvider.GetRequiredService<IPushNotificationTokenStore>();
            var tokens = await store.GetTokensAsync(userId);
            Assert.Empty(tokens);
        }
    }
}
