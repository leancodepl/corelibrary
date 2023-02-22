using Autofac;
using LeanCode.Components;

namespace LeanCode.OpenTelemetry;

public class OpenTelemetryModule : AppModule
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(TracingElement<,,>));
    }
}
