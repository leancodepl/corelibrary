using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace LeanCode.ClientCredentialsHandler
{
    // Based on https://github.com/IdentityModel/IdentityModel2/blob/dev/src/IdentityModel/Client/RefreshTokenHandler.cs
    public class ClientCredentialsHandler : DelegatingHandler
    {
        private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ClientCredentialsHandler>();

        private readonly ClientCredentialsConfiguration config;
        private readonly SemaphoreSlim tokenLock = new SemaphoreSlim(1, 1);
        private readonly TokenClient tokenClient;

        private string accessToken = null;

        private bool disposed = false;

        private string AccessToken
        {
            get
            {
                if (tokenLock.Wait(LockTimeout))
                {
                    try
                    {
                        return accessToken;
                    }
                    finally
                    {
                        tokenLock.Release();
                    }
                }

                return null;
            }
        }

        public ClientCredentialsHandler(ClientCredentialsConfiguration config)
            : base(new HttpClientHandler())
        {
            this.config = config;
            var uri = BuildTokenUrl(config.ServerAddress);
            this.tokenClient = new TokenClient(uri, config.ClientId, config.ClientSecret);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = AccessToken;
            if (token == null)
            {
                if (!await GetNewAccessToken(cancellationToken))
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            if (!await GetNewAccessToken(cancellationToken))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;
                tokenClient.Dispose();
                tokenLock.Dispose();
            }

            base.Dispose(disposing);
        }

        private async Task<bool> GetNewAccessToken(CancellationToken cancellationToken)
        {
            if (await tokenLock.WaitAsync(LockTimeout, cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    logger.Debug("Requesting access token");
                    var response = await tokenClient
                        .RequestClientCredentialsAsync(
                            config.Scopes, cancellationToken: cancellationToken);
                    if (!response.IsError)
                    {
                        logger.Information("New access token retrieved");
                        accessToken = response.AccessToken;
                        return true;
                    }
                    else
                    {
                        logger.Fatal(
                            "Cannot get access token - server rejected the request with error {Error}",
                            response.ErrorDescription);
                    }
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex, "Cannot connecto to auth server");
                    throw;
                }
                finally
                {
                    tokenLock.Release();
                }
            }
            return false;
        }

        private static string BuildTokenUrl(string address)
        {
            if (address.EndsWith("/"))
            {
                return address + "connect/token";
            }
            else
            {
                return address + "/connect/token";
            }
        }
    }
}
