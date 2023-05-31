using IdentityServer4.Models;
using static IdentityServer4.IdentityServerConstants;
using AuthConsts = ExampleApp.Core.Contracts.Auth;

namespace ExampleApp.Api.Auth;

internal static class ClientsConfiguration
{
    public static List<Client> GetClients()
    {
        return new List<Client>
        {
            new Client
            {
                ClientId = AuthConsts.Clients.AdminApp,
                ClientName = "ExampleApp Admin App",
                AllowedGrantTypes = new[] { GrantType.ResourceOwnerPassword },
                AllowOfflineAccess = true,
                RequireClientSecret = false,

                AbsoluteRefreshTokenLifetime = (int)TimeSpan.FromDays(365 * 10).TotalSeconds,

                AllowedScopes = new List<string>
                {
                    StandardScopes.OpenId,
                    StandardScopes.Profile,
                    StandardScopes.Email,
                    StandardScopes.Phone,
                    StandardScopes.OfflineAccess,

                    AuthConsts.Scopes.InternalApi,
                },
            },
            new Client
            {
                ClientId = AuthConsts.Clients.ClientApp,
                ClientName = "ExampleApp Client App",
                AllowedGrantTypes = new[] { GrantType.ResourceOwnerPassword },
                AllowOfflineAccess = true,
                RequireClientSecret = false,

                AbsoluteRefreshTokenLifetime = (int)TimeSpan.FromDays(365 * 10).TotalSeconds,

                AllowedScopes = new List<string>
                {
                    StandardScopes.OpenId,
                    StandardScopes.Profile,
                    StandardScopes.Email,
                    StandardScopes.Phone,
                    StandardScopes.OfflineAccess,

                    AuthConsts.Scopes.InternalApi,
                },
            },
        };
    }

    public static List<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone(),
        };
    }

    public static List<ApiResource> GetApiResources()
    {
        return new List<ApiResource>
        {
            new ApiResource(AuthConsts.Scopes.InternalApi, "Internal Api")
            {
                Scopes = { AuthConsts.Scopes.InternalApi },
                UserClaims = { AuthConsts.KnownClaims.Role },
            },
        };
    }

    public static List<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            new ApiScope(AuthConsts.Scopes.InternalApi, "Full access to the ExampleApp Api")
            {
                UserClaims = { AuthConsts.KnownClaims.Role },
            },
        };
    }
}
