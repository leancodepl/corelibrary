# Observability - OpenTelemetry

LeanCode CoreLibrary simplifies the implementation of observability practices by integrating with OpenTelemetry. This integration empowers developers to effortlessly incorporate distributed tracing, metrics collection, and logging functionalities into their applications. Leveraging OpenTelemetry's instrumentation capabilities, CoreLibrary enables comprehensive monitoring and analysis across various services and components within the application architecture. For further exploration of OpenTelemetry's features, refer to [OpenTelemetry's documentation](https://opentelemetry.io/docs/).

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.CQRS.AspNetCore | [![NuGet version (LeanCode.CQRS.AspNetCore)](https://img.shields.io/nuget/vpre/LeanCode.CQRS.AspNetCore.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.AspNetCore/8.0.2260-preview/) | `CQRSTrace()` |
| LeanCode.OpenTelemetry | [![NuGet version (LeanCode.OpenTelemetry)](https://img.shields.io/nuget/vpre/LeanCode.OpenTelemetry.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.OpenTelemetry/8.0.2260-preview/) | CoreLibrary traces/metrics configuration |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | [![NuGet version (OpenTelemetry.Exporter.OpenTelemetryProtocol)](https://img.shields.io/nuget/v/OpenTelemetry.Exporter.OpenTelemetryProtocol.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/OpenTelemetry.Exporter.OpenTelemetryProtocol/1.6.0) | `AddOtlpExporter(...)` |
| OpenTelemetry.Extensions.Hosting | [![NuGet version (OpenTelemetry.Extensions.Hosting)](https://img.shields.io/nuget/v/OpenTelemetry.Extensions.Hosting.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting/1.6.0) | OpenTelemetry extension methods |
| OpenTelemetry.Instrumentation.AspNetCore | [![NuGet version (OpenTelemetry.Instrumentation.AspNetCore)](https://img.shields.io/nuget/v/OpenTelemetry.Instrumentation.AspNetCore.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AspNetCore/1.6.0-beta.3) | `AddAspNetCoreInstrumentation()` |
| OpenTelemetry.Instrumentation.Http | [![NuGet version (OpenTelemetry.Instrumentation.Http)](https://img.shields.io/nuget/v/OpenTelemetry.Instrumentation.Http.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.Http/1.6.0-beta.3) | `AddHttpClientInstrumentation()` |

## Configuration

The code snippet below orchestrates OpenTelemetry setup in a .NET application. It sets up tracing and metrics collection, integrating with the CoreLibrary. The `IdentityTraceAttributesFromBaggageProcessor` custom processor retrieves and sets user identity information and roles from activity baggage. The provided code uses an OpenTelemetry exporter endpoint and adds instrumentation for tracing and metrics, utilizing the OTLP exporter to send telemetry data to a specified endpoint if the endpoint is provided.

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // . . .

    // OpenTelemetry exporter endpoint.
    var otlp = "";

    if (!string.IsNullOrWhiteSpace(otlp))
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(
                serviceName: "ExampleApp.Api",
                serviceInstanceId: Environment.MachineName))
            .WithTracing(builder =>
            {
                builder
                    .AddProcessor<IdentityTraceAttributesFromBaggageProcessor>()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Collect CoreLibrary traces - cqrs contract's
                    // full name if CQRSTracingMiddleware was added.
                    .AddLeanCodeTelemetry()
                    .AddOtlpExporter(cfg => cfg.Endpoint = new(otlp));
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Collect CoreLibrary metrics - cqrs success/failure
                    // counters with reason when failed.
                    .AddLeanCodeMetrics()
                    .AddOtlpExporter(cfg => cfg.Endpoint = new(otlp));
            });
    }

    // . . .
}
```

To enable the inclusion of identity details and roles within the `IdentityTraceAttributesFromBaggageProcessor`, it is necessary to add this information into the activity baggage. The code snippet below adds identity information and roles to the activity baggage through the use of middleware.

```csharp
protected override void ConfigureApp(IApplicationBuilder app)
{
    // . . .

    app.UseAuthentication()
        .UseIdentityTraceAttributes(
            userIdClaim: "sub",
            roleClaim: "role");

    // . . .
}
```

To enable CQRS traces in your application, it's necessary to use the `CQRSTrace()` method which adds `CQRSTracingMiddleware` when setting up the pipeline.

```csharp
    protected override void ConfigureApp(IApplicationBuilder app)
    {
        // . . .

        app.UseEndpoints(endpoints =>
            {
                endpoints.MapRemoteCQRS(
                    "/api",
                    cqrs =>
                    {
                        cqrs.Commands = c =>
                            c.CQRSTrace()
                            // . . .

                        cqrs.Queries = c =>
                            c.CQRSTrace()
                            // . . .

                        cqrs.Operations = c =>
                            c.CQRSTrace()
                            // . . .
                    }
                );
            });

        // . . .
    }
```

!!! tip
    To learn more about configuring the pipeline, visit [here](../../cqrs/pipeline/index.md).
