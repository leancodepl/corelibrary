using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeanCode.OpenTelemetry;

public class OpenTelemetryModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddTransient(typeof(TracingElement<,,>));
    }
}
