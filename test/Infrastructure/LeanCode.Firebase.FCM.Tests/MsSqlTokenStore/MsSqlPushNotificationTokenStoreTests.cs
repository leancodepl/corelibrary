using LeanCode.Firebase.FCM.SqlServer;
using Xunit;

namespace LeanCode.Firebase.FCM.Tests.MsSqlTokenStore;

public class MSSQLPushNotificationTokenStoreTests : IAsyncLifetime
{
    private SqliteTestDbContext context = default!;
    private MsSqlPushNotificationTokenStore<SqliteTestDbContext> store = default!;

    [Fact]
    public async Task Gets_freshly_saved_token()
    {
        const string Token = "token";
        var uid = Guid.NewGuid();

        await store.AddUserTokenAsync(uid, Token);
        var result = await store.GetTokensAsync(uid);

        Assert.Equal(new() { Token }, result);
    }

    [Fact]
    public async Task Gets_multiple_freshly_saved_token()
    {
        const string Token1 = "token1";
        const string Token2 = "token2";
        var uid = Guid.NewGuid();

        await store.AddUserTokenAsync(uid, Token1);
        await store.AddUserTokenAsync(uid, Token2);
        var result = await store.GetTokensAsync(uid);

        Assert.Equal(new() { Token1, Token2 }, result);
    }

    [Fact]
    public async Task Gets_tokens_for_multiple_users()
    {
        const string Token1 = "token1";
        const string Token2 = "token2";
        var uid1 = Guid.NewGuid();
        var uid2 = Guid.NewGuid();

        await store.AddUserTokenAsync(uid1, Token1);
        await store.AddUserTokenAsync(uid2, Token2);

        var result = await store.GetTokensAsync(new HashSet<Guid> { uid1, uid2 });

        Assert.Equal(
            new()
            {
                [uid1] = new() { Token1 },
                [uid2] = new() { Token2 },
            },
            result);
    }

    [Fact]
    public async Task Removes_single_token()
    {
        const string Token1 = "token1";
        const string Token2 = "token2";
        var uid = Guid.NewGuid();

        await store.AddUserTokenAsync(uid, Token1);
        await store.AddUserTokenAsync(uid, Token2);
        await store.RemoveTokenAsync(Token1);
        var result = await store.GetTokensAsync(uid);

        Assert.Equal(new() { Token2 }, result);
    }

    [Fact]
    public async Task Removes_user_tokens()
    {
        const string Token1 = "token1";
        const string Token2 = "token2";
        var uid1 = Guid.NewGuid();
        var uid2 = Guid.NewGuid();

        await store.AddUserTokenAsync(uid1, Token1);
        await store.AddUserTokenAsync(uid2, Token2);

        await store.RemoveUserTokenAsync(uid1, Token2); // Noop
        await store.RemoveUserTokenAsync(uid1, Token1);

        var result1 = await store.GetTokensAsync(uid1);
        var result2 = await store.GetTokensAsync(uid2);

        Assert.Empty(result1);
        Assert.Equal(new() { Token2 }, result2);
    }

    [Fact]
    public async Task Removes_multiple_tokens()
    {
        const string Token1 = "token1";
        const string Token2 = "token2";
        var uid1 = Guid.NewGuid();
        var uid2 = Guid.NewGuid();

        await store.AddUserTokenAsync(uid1, Token1);
        await store.AddUserTokenAsync(uid2, Token2);

        await store.RemoveTokensAsync(new[] { Token1, Token2 });

        var result1 = await store.GetTokensAsync(uid1);
        var result2 = await store.GetTokensAsync(uid2);

        Assert.Empty(result1);
        Assert.Empty(result2);
    }

    public async Task InitializeAsync()
    {
        context = await SqliteTestDbContext.CreateInMemory();
        store = new MsSqlPushNotificationTokenStore<SqliteTestDbContext>(context);
    }

    public async Task DisposeAsync()
    {
        await context.DisposeAsync();
    }
}
