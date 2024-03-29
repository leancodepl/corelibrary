using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LeanCode.CQRS.MassTransitRelay;
using LeanCode.CQRS.RemoteHttp.Client;
using LeanCode.Serialization;
using MassTransit.Testing.Implementations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.IntegrationTestHelpers;

public abstract class LeanCodeTestFactory<TStartup> : WebApplicationFactory<TStartup>, IAsyncLifetime
    where TStartup : class
{
    protected abstract ConfigurationOverrides Configuration { get; }

    public virtual JsonSerializerOptions JsonOptions { get; }

    protected virtual string ApiBaseAddress => "api/";

    protected LeanCodeTestFactory()
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters =
            {
                new JsonLaxDateOnlyConverter(),
                new JsonLaxTimeOnlyConverter(),
                new JsonLaxDateTimeOffsetConverter(),
            },
        };

        options.MakeReadOnly();

        JsonOptions = options;
    }

    public virtual HttpClient CreateApiClient(Action<HttpClient>? config = null)
    {
        var apiBase = UrlHelper.Concat("http://localhost/", ApiBaseAddress);
        var client = CreateDefaultClient(new Uri(apiBase));
        config?.Invoke(client);
        return client;
    }

    public virtual HttpQueriesExecutor CreateQueriesExecutor(Action<HttpClient>? config = null)
    {
        return new HttpQueriesExecutor(CreateApiClient(config), JsonOptions);
    }

    public virtual HttpCommandsExecutor CreateCommandsExecutor(Action<HttpClient>? config = null)
    {
        return new HttpCommandsExecutor(CreateApiClient(config), JsonOptions);
    }

    public virtual HttpOperationsExecutor CreateOperationsExecutor(Action<HttpClient>? config = null)
    {
        return new HttpOperationsExecutor(CreateApiClient(config), JsonOptions);
    }

    public virtual async Task WaitForBusAsync(TimeSpan? delay = null, TimeSpan? outboxDelay = null)
    {
        // We cannot explicitly wait until outbox service has published events
        await Task.Delay(outboxDelay ?? TimeSpan.FromSeconds(1));

        var busInactive = await Services
            .GetRequiredService<IBusActivityMonitor>()
            .AwaitBusInactivity(delay ?? TimeSpan.FromSeconds(5));

        if (!busInactive)
        {
            throw new System.InvalidOperationException("Bus did not stabilize");
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
                // Allow the host to perform shutdown a little bit longer - it will make
                // `DbContextsInitializer` successfully drop the database more frequently. :)
                services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));

                services.AddBusActivityMonitor();
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
