using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace LeanCode.IdentityServer.KeyVault
{
    // Based on
    // https://github.com/IdentityServer/IdentityServer4/blob/75ac815/src/IdentityServer4/Services/DefaultTokenCreationService.cs
    class TokenCreationService : ITokenCreationService
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<TokenCreationService>();

        private readonly SigningService signing;

        public TokenCreationService(SigningService signing)
        {
            this.signing = signing;
        }

        public async Task<string> CreateTokenAsync(Token token)
        {
            var credentials = await signing.GetSigningCredentialsAsync();

            var header = CreateHeaderAsync(credentials);
            var payload = CreatePayloadAsync(token);
            var jwt = new JwtSecurityToken(header, payload);

            return await signing.SignTokenAsync(jwt);
        }

        private JwtHeader CreateHeaderAsync(SigningCredentials credentials)
        {
            var header = new JwtHeader(credentials);

            // emit x5t claim for backwards compatibility with v4 of MS JWT library
            if (credentials.Key is X509SecurityKey x509key)
            {
                var cert = x509key.Certificate;
                if (DateTime.UtcNow > cert.NotAfter)
                {
                    logger.Warning(
                        "Certificate {SubjectName} has expired on {Expiration}",
                        cert.Subject, cert.NotAfter.ToString());
                }

                header.Add("x5t", Base64Url.Encode(cert.GetCertHash()));
            }

            return header;
        }

        private JwtPayload CreatePayloadAsync(Token token)
        {
            var payload = new JwtPayload(
                token.Issuer,
                null,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddSeconds(token.Lifetime));

            foreach (var aud in token.Audiences)
            {
                payload.AddClaim(new Claim(JwtClaimTypes.Audience, aud));
            }

            var amrClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.AuthenticationMethod);
            var scopeClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.Scope);
            var jsonClaims = token.Claims.Where(x => x.ValueType == IdentityServerConstants.ClaimValueTypes.Json);

            var normalClaims = token.Claims
                .Except(amrClaims)
                .Except(jsonClaims)
                .Except(scopeClaims);

            payload.AddClaims(normalClaims);

            // scope claims
            if (!scopeClaims.IsNullOrEmpty())
            {
                var scopeValues = scopeClaims.Select(x => x.Value).ToArray();
                payload.Add(JwtClaimTypes.Scope, scopeValues);
            }

            // amr claims
            if (!amrClaims.IsNullOrEmpty())
            {
                var amrValues = amrClaims.Select(x => x.Value).Distinct().ToArray();
                payload.Add(JwtClaimTypes.AuthenticationMethod, amrValues);
            }

            // deal with json types
            // calling ToArray() to trigger JSON parsing once and so later
            // collection identity comparisons work for the anonymous type
            var jsonTokens = jsonClaims.Select(x => new { x.Type, JsonValue = JRaw.Parse(x.Value) }).ToArray();

            var jsonObjects = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Object).ToArray();
            var jsonObjectGroups = jsonObjects.GroupBy(x => x.Type).ToArray();
            foreach (var group in jsonObjectGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new Exception(String.Format("Can't add two claims where one is a JSON object and the other is not a JSON object ({0})", group.Key));
                }

                if (group.Skip(1).Any())
                {
                    // add as array
                    payload.Add(group.Key, group.Select(x => x.JsonValue).ToArray());
                }
                else
                {
                    // add just one
                    payload.Add(group.Key, group.First().JsonValue);
                }
            }

            var jsonArrays = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Array).ToArray();
            var jsonArrayGroups = jsonArrays.GroupBy(x => x.Type).ToArray();
            foreach (var group in jsonArrayGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new Exception(String.Format("Can't add two claims where one is a JSON array and the other is not a JSON array ({0})", group.Key));
                }

                List<JToken> newArr = new List<JToken>();
                foreach (var arrays in group)
                {
                    var arr = (JArray)arrays.JsonValue;
                    newArr.AddRange(arr);
                }

                // add just one array for the group/key/claim type
                payload.Add(group.Key, newArr.ToArray());
            }

            var unsupportedJsonTokens = jsonTokens.Except(jsonObjects).Except(jsonArrays);
            var unsupportedJsonClaimTypes = unsupportedJsonTokens.Select(x => x.Type).Distinct();
            if (unsupportedJsonClaimTypes.Any())
            {
                throw new Exception(String.Format("Unsupported JSON type for claim types: {0}", unsupportedJsonClaimTypes.Aggregate((x, y) => x + ", " + y)));
            }

            return payload;
        }
    }
}
