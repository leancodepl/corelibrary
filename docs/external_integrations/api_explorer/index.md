# API Explorer/Swagger integration

[CQRS](../../cqrs/index.md) implementation integrates seamlessly with the [OpenAPI](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview) support of ASP.NET Core. This means that every endpoint can be automatically documented by other tools that leverage it, e.g. [SwaggerUI](https://swagger.io/tools/swaggerhub/) or [ReDoc](https://github.com/Redocly/redoc).

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.CQRS.AspNetCore | [![NuGet version (LeanCode.CQRS.AspNetCore)](https://img.shields.io/nuget/vpre/LeanCode.CQRS.AspNetCore.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.CQRS.AspNetCore) | Configuration |

## Configuration

To add the service, call the `AddCQRSApiExplorer` extension method in `ConfigureServices`:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    services.AddCQRSApiExplorer();
}
```

The `AddCQRSApiExplorer` method has an optional parameter that accepts a configuration override.
It allows configuring what metadata is passed to the OpenAPI tooling, including:

1. Tags,
2. Summary,
3. Description.

See the [implementation] for defaults.

To better accomodate common tag patterns, we also provide [ApiDescriptionTags] class with predefined tag generation based on the namespace of the command/query/operation.

[implementation]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.AspNetCore/Registration/CQRSApiDescriptionConfiguration.cs
[ApiDescriptionTags]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/CQRS/LeanCode.CQRS.AspNetCore/Registration/ApiDescriptionTags.cs
