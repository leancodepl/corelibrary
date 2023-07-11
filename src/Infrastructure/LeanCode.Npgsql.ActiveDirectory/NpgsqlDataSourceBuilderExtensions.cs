using Azure.Core;
using Npgsql;

namespace LeanCode.Npgsql.ActiveDirectory;

public static class NpgsqlDataSourceBuilderExtensions
{
    private static readonly string[] Scopes = new[] { "https://ossrdbms-aad.database.windows.net/.default" };

    public static NpgsqlDataSourceBuilder UseAzureActiveDirectoryAuthentication(
        this NpgsqlDataSourceBuilder builder,
        TokenCredential credential
    )
    {
        return builder.UsePeriodicPasswordProvider(
            async (_, ct) =>
            {
                var token = await credential.GetTokenAsync(new(Scopes, null), ct);

                return token.Token;
            },
            TimeSpan.FromMinutes(5),
            TimeSpan.FromSeconds(1)
        );
    }
}
