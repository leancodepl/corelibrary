using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4.Models;
using IdentityServer4.Test;
using static IdentityServer4.IdentityServerConstants;

namespace LeanCode.IntegrationTestHelpers.Tests.App
{
    public static class AuthConfig
    {
        public const string Username = "test@leancode.pl";
        public const string Password = "Food1App!";
        public static readonly Guid UserId = Guid.NewGuid();

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = UserId.ToString(),
                    Username = Username,
                    Password = Password,
                    IsActive = true,
                    Claims = { new Claim("role", "user") },
                },
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client()
                {
                    ClientId = "web",
                    ClientName = "Test web client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    RequireClientSecret = false,

                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                        StandardScopes.Email,
                        StandardScopes.Phone,

                        "api",
                    },
                },
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Phone(),
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
                new ApiResource("api", "API", new[] { "role" }),
            };
        }

        public static List<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope("api", "API")
                {
                    UserClaims = { "role" },
                },
            };
        }
    }
}
