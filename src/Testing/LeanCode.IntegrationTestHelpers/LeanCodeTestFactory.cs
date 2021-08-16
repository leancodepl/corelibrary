using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using LeanCode.CQRS.RemoteHttp.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.IntegrationTestHelpers
{
    public abstract class LeanCodeTestFactory<TStartup> : WebApplicationFactory<TStartup>, IAsyncLifetime
        where TStartup : class
    {
        protected virtual ConfigurationOverrides Configuration { get; } = new ConfigurationOverrides();
        protected virtual string ApiBaseAddress => "api/";
        protected virtual string AuthBaseAddress => "auth/";

        public string? CurrentUserToken { get; private set; }

        public HttpClient CreateApiClient()
        {
            var apiBase = UrlHelper.Concat("http://localhost/", ApiBaseAddress);
            var client = CreateDefaultClient(new Uri(apiBase));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentUserToken);
            return client;
        }

        public HttpClient CreateAuthClient()
        {
            var apiBase = UrlHelper.Concat("http://localhost/", AuthBaseAddress);
            return CreateDefaultClient(new Uri(apiBase));
        }

        public HttpQueriesExecutor CreateQueriesExecutor()
        {
            return new HttpQueriesExecutor(CreateApiClient());
        }

        public HttpCommandsExecutor CreateCommandsExecutor()
        {
            return new HttpCommandsExecutor(CreateApiClient());
        }

        public virtual async Task<bool> AuthenticateAsync(PasswordTokenRequest tokenRequest)
        {
            // FIXME: what if the auth server is outside of the app?
            using var client = CreateAuthClient();

            var discovery = await client.GetDiscoveryDocumentAsync();
            if (discovery.IsError)
            {
                return false;
            }
            else
            {
                tokenRequest.Address = discovery.TokenEndpoint;

                var token = await client.RequestPasswordTokenAsync(tokenRequest);
                if (token.IsError)
                {
                    return false;
                }
                else
                {
                    CurrentUserToken = token.AccessToken;
                    return true;
                }
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .ConfigureAppConfiguration(config =>
                {
                    config.Add(Configuration);
                })
                .ConfigureServices(services =>
                {
                    services.Configure<JwtBearerOptions>(
                        JwtBearerDefaults.AuthenticationScheme,
                        opts => opts.BackchannelHttpHandler = Server.CreateHandler());
                    // Allow the host to perform shutdown a little bit longer - it will make
                    // `DbContextsInitializer` successfully drop the database more frequently. :)
                    services.Configure<HostOptions>(
                            opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));
                });
        }

        public virtual async Task InitializeAsync()
        {
            // Since we can't really run async variants of the Start/Stop methods of webhost
            // (neither TestServer nor WebApplicationFactory allows that - they just Start using
            // sync-over-async approach), we at least do that in controlled manner.
            await Task.Run(() => Server.Services.ToString()).ConfigureAwait(false);
        }

        Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
        }
    }
}
