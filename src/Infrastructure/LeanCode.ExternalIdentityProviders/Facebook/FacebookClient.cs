using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCode.ExternalIdentityProviders.Facebook
{
    public class FacebookClient
    {
        public const string ApiBase = "https://graph.facebook.com";
        public const string ApiVersion = "v8.0";

        private const string FieldsStr = "id,email,first_name,last_name";

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<FacebookClient>();

        private readonly HttpClient client;
        private readonly HMACSHA256 hmac;

        private readonly int photoSize;

        public FacebookClient(FacebookConfiguration config, HttpClient client)
        {
            this.client = client;

            photoSize = config.PhotoSize;
            hmac = new HMACSHA256(ParseKey(config.AppSecret));
        }

        public virtual async Task<JsonDocument> CallAsync(
            string endpoint,
            string accessToken,
            bool handleError = true,
            CancellationToken cancellationToken = default)
        {
            var proof = GenerateProof(accessToken);
            var uri = AppendProof(endpoint, accessToken, proof);

            try
            {
                using var response = await client.GetAsync(uri, cancellationToken);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    logger.Warning(
                        "Facebook Graph API returned {StatusCode} {reasonPhrase} for access token {FBAccessToken}",
                        response.StatusCode, response.ReasonPhrase, accessToken);

                    throw new FacebookException($"Cannot call Facebook Graph API, status: {response.StatusCode}.");
                }

                await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
                var result = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);

                if (handleError && result.RootElement.TryGetProperty("error", out var error))
                {
                    using (result)
                    {
                        var code = error.GetProperty("code").GetInt32();
                        var msg = error.GetProperty("message").GetString();

                        logger.Warning(
                            "User tried to use invalid access token {FBAccessToken}, Facebook returned {Code} error code with message {Message}",
                            accessToken, code, msg);

                        throw new FacebookException(
                            $"Cannot call Facebook Graph API. The call resulted in error {code} with message {msg}.");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Cannot call Facebook Graph API");

                throw new FacebookException("Cannot call Facebook Graph API.", ex);
            }
        }

        public virtual async Task<FacebookUser> GetUserInfoAsync(string accessToken)
        {
            using var result = await CallAsync("me?fields=" + FieldsStr, accessToken);

            var info = ConvertResponse(result.RootElement);

            logger.Information(
                "Facebook user retrieved, user {FBUserId} has e-mail {FBEmail}",
                info.Id, info.Email);

            return info;
        }

        private FacebookUser ConvertResponse(JsonElement result)
        {
            var id = result.GetProperty("id").GetString()!;
            var email = GetOptionalProperty(result, "email");
            var firstName = GetOptionalProperty(result, "first_name");
            var lastName = GetOptionalProperty(result, "last_name");
            var photoUrl = $"{ApiBase}/{ApiVersion}/{id}/picture?width={photoSize}&height={photoSize}";

            return new FacebookUser(id, email, firstName, lastName, photoUrl);

            static string? GetOptionalProperty(JsonElement element, string propName)
            {
                if (element.TryGetProperty(propName, out var prop))
                {
                    return prop.ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        private string GenerateProof(string accessToken)
        {
            logger.Debug("Signing Facebook request enabled - signing request");

            var bytes = ParseKey(accessToken);
            var hash = hmac.ComputeHash(bytes);

            return "&appsecret_proof=" + ToHexString(hash);
        }

        private static byte[] ParseKey(string v) =>
            Encoding.ASCII.GetBytes(v);

        private static string ToHexString(byte[] data) =>
            BitConverter.ToString(data).Replace("-", string.Empty).ToLower();

        private static string AppendProof(string uri, string accessToken, string proof)
        {
            return uri.Contains("?")
                ? $"{ApiVersion}/{uri}&access_token={accessToken}{proof}"
                : $"{ApiVersion}/{uri}?access_token={accessToken}{proof}";
        }
    }
}
