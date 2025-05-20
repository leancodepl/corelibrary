using System.Net;
using System.Net.Http.Headers;
using IdentityModel.Client;
using LeanCode.Logging;

namespace LeanCode.ClientCredentialsHandler;

// Based on https://github.com/IdentityModel/IdentityModel2/blob/dev/src/IdentityModel/Client/RefreshTokenHandler.cs
public class ClientCredentialsHandler : DelegatingHandler
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);

    private readonly string tokenEndpoint;
    private readonly ClientCredentialsConfiguration config;
    private readonly SemaphoreSlim tokenLock = new SemaphoreSlim(1, 1);
    private readonly HttpClient httpClient;
    private readonly ILogger<ClientCredentialsHandler> logger;

    private string? accessToken;

    private bool disposed;

    private string? AccessToken
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2000", Justification = "Disposed by parent class.")]
    public ClientCredentialsHandler(ClientCredentialsConfiguration config, ILogger<ClientCredentialsHandler> logger)
        : base(new HttpClientHandler())
    {
        this.config = config;
        this.logger = logger;

        httpClient = new HttpClient();
        tokenEndpoint = UrlHelper.Concat(config.ServerAddress, "connect/token");
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (AccessToken is null)
        {
            if (!await GetNewAccessTokenAsync(cancellationToken))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        HttpResponseMessage? response = null;

        try
        {
            response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }
        }
        finally
        {
            if (response?.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
            }
        }

        if (!await GetNewAccessTokenAsync(cancellationToken))
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            disposed = true;

            httpClient.Dispose();
            tokenLock.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task<bool> GetNewAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (await tokenLock.WaitAsync(LockTimeout, cancellationToken))
        {
            try
            {
                logger.Debug("Requesting access token");

                using var request = new ClientCredentialsTokenRequest
                {
                    Address = tokenEndpoint,
                    ClientId = config.ClientId,
                    ClientSecret = config.ClientSecret,
                    Scope = config.Scopes,
                };
                var response = await httpClient.RequestClientCredentialsTokenAsync(request, cancellationToken);

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
                        response.ErrorDescription
                    );

                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Cannot connect to auth server");

                throw;
            }
            finally
            {
                tokenLock.Release();
            }
        }
        else
        {
            return false;
        }
    }
}
