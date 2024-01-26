# API Explorer/Swagger integration

[CQRS](../../cqrs/index.md) implementation integrates seamlessly with the [API Explorer](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.apiexplorer) functionality of ASP.NET Core. This means that every endpoint can be automatically documented by other tools that leverage it, e.g. [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) or [NSwag](https://github.com/RicoSuter/NSwag).

By using these tools, you get OpenAPI definitions, along the [Swagger UI](https://swagger.io/tools/swagger-ui/) tooling.

The integration is available out of the box for every exposed command, query or operation.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.CQRS.AspNetCore | [![NuGet version (LeanCode.CQRS.AspNetCore)](https://img.shields.io/nuget/vpre/LeanCode.CQRS.AspNetCore.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.AspNetCore) | Configuration |

## JSON Casing

Every integration uses different configuration for the generated payload types. It is highly probable that the default configuration of JSON serializer for these integrations will use wrong property casing. Unfortunately, every tool is configured differently.

### Swashbuckle

To configure Swashbuckle, you need to register custom `ISerializerDataContractResolver` that has the same configurtion as the `ISerializer` of CQRS.

```csharp
// This will use default JSON serialization, with unchanged property names (this is the default of System.Text.Json) & CQRS
builder
    .Services
    .AddTransient<ISerializerDataContractResolver>(
        _ => new JsonSerializerDataContractResolver(new JsonSerializerOptions { PropertyNamingPolicy = null })
    );
```
