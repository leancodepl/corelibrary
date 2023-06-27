using System.Net.Http.Headers;
using System.Security.Claims;

namespace LeanCode.IntegrationTestHelpers;

public static class HttpClientExtensions
{
    public static HttpClient UseTestAuthorization(this HttpClient client, ClaimsPrincipal principal)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            TestAuthenticationHandler.SchemeName,
            TestAuthenticationHandler.SerializePrincipal(principal)
        );
        return client;
    }
}
