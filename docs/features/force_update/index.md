# Force update

CoreLibrary provides an opinionated library for adding force update support to mobile apps. It connects to the broaded CoreLibrary ecosystem and has a ready-made [Flutter implementation](https://github.com/leancodepl/flutter_corelibrary/tree/master/packages/force_update).

## Configuration

To enforce or suggest updates for client apps, you can utilize the `AddForceUpdate(...)` extension method from the `LeanCode.ForceUpdate` package in the `Startup.cs` file. This method is available on the `IServiceCollection` and needs to be called after `AddCQRS(...)`. The following example demonstrates the usage:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddCQRS(CQRSTypes, CQRSTypes)
            .AddForceUpdate(
                new AndroidVersionsConfiguration(
                    new Version(AndroidMinimumRequiredVersion),
                    new Version(AndroidCurrentlySupportedVersion)),
                new IOSVersionsConfiguration(
                    new Version(IOSMinimumRequiredVersion),
                    new Version(IOSCurrentlySupportedVersion)));
    // ...
}
```

## Version support

After configuation above. [VersionSupport] query is created and available at `/cqrs/query/LeanCode.ForceUpdate.Contracts.VersionSupport`. The query takes `Platform` (either IOS or Android) and the `Version` of the client app as parameters. It returns whether the client's version is supported, currently supported version and minimum required version for specified provider.

By default, if the client's version is below the minimum required version, the response will indicate that an update is needed. If the client's version is between minimum required and currently supported version, the response will suggest an update. If the app version is greater or equal to the currently supported version, the response will indicate that the app is up to date. It's also possible to change this behavior by creating custom version handler and overriding `CheckVersionAsync` method from the [VersionHandler] class, responsible for version checking (for example when we want to force only specific group of users to update the app).

[VersionSupport]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/Infrastructure/LeanCode.ForceUpdate.Contracts/VersionSupport.cs
[VersionHandler]: https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/Infrastructure/LeanCode.ForceUpdate/LeanCode.ForceUpdate.Services/VersionHandler.cs
