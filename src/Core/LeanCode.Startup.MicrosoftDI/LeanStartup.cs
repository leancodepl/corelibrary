using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LeanCode.Startup.MicrosoftDI;

public abstract class LeanStartup
{
    protected IConfiguration Configuration { get; }

    protected virtual bool CloseAndFlushLogger { get; } = true;

    protected LeanStartup(IConfiguration config)
    {
        Configuration = config;
    }

    public abstract void ConfigureServices(IServiceCollection services);

    public void Configure(IApplicationBuilder app)
    {
        ConfigureApp(app);

        if (CloseAndFlushLogger)
        {
            var appLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        }
    }

    protected abstract void ConfigureApp(IApplicationBuilder app);
}
