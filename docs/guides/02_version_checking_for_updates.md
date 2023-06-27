# Forced/suggested updates

## Configuring version checks

To enforce or suggest updates for client apps, you can utilize the `AddForceUpdate()` extension method from the `LeanCode.CQRS.AspNetCore` package in the `Startup.cs` file. This method is available on the `IServiceCollection`. To make the endpoints work, you need to register two records: `IOSVersionsConfiguration` and `AndroidVersionsConfiguration`. Both require the `MinimumRequiredVersion` and `CurrentlySupportedVersion` parameters. The following example demonstrates the usage:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddCQRS(CQRSTypes, CQRSTypes);
    services.AddForceUpdate();

    services.AddSingleton(
        new IOSVersionsConfiguration(
            new Version(IOSMinimumRequiredVersion),
            new Version(IOSCurrentlySupportedVersion)
        )
    );
    services.AddSingleton(
        new AndroidVersionsConfiguration(
            new Version(AndroidMinimumRequiredVersion),
            new Version(AndroidCurrentlySupportedVersion)
        )
    );
    // ...
}
```

## Version support
After configuation above. `VersionSupport` query is created and available at `/cqrs/query/LeanCode.ForceUpdate.Contracts.VersionSupport`. This endpoint should be used by default:
```csharp
public class VersionSupport : IQuery<VersionSupportDTO?>
{
    public PlatformDTO Platform { get; set; }
    public string Version { get; set; } = default!;
}

public enum VersionSupportDTO
{
    UpdateRequired,
    UpdateSuggested,
    UpToDate,
}

public enum PlatformDTO
{
    IOS,
    Android,
}
```
The query takes `Platform` (either IOS or Android) and the `Version` of the client app as parameters. It returns whether the client's version is supported. If the client's version is below the `MinimumRequiredVersion` defined earlier, the response will indicate that an update is needed. If the client's version is greater than or equal to the `MinimumRequiredVersion` and less than the `CurrentlySupportedVersion`, the response will suggest an update. If the app version is greater than or equal to the `CurrentlySupportedVersion`, the response will indicate that the app is up to date. It's also possible to override this behavior by overriding the `CheckVersion` method in the `VersionHandler` class, responsible for version checking:

```csharp
public class VersionHandler
{
    public virtual VersionSupport CheckVersion(
        Version version,
        Version minimumRequiredVersion,
        Version currentlySupportedVersion,
        HttpContext context
    )
    {
    // ...
    }

    public enum VersionSupport
    {
        UpdateRequired,
        UpdateSuggested,
        UpToDate,
    }
}
```
Below is an example of overriding the `CheckVersion` method:
```csharp
public class CustomVersionHandler : VersionHandler
{
    public override VersionSupport CheckVersion(
        Version version,
        Version minimumRequiredVersion,
        Version currentlySupportedVersion,
        HttpContext context
    )
    {
    // You can also use for example UserId from HttpContext
    }

    public enum VersionSupport
    {
        UpdateRequired,
        UpdateSuggested,
        UpToDate,
    }
}
```
To make it work, you need to register `CustomVersionHandler` in the ConfigureServices method:
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    //...
    services.AddTransient<VersionHandler, CustomVersionHandler>();
    //..
}
```

## Versions

After the configuration, a `Versions` query is also created and available at the `/cqrs/query/LeanCode.ForceUpdate.Contracts.Versions` endpoint. The query returns a list of supported app versions for all platforms.

```csharp
public class Versions : IQuery<List<VersionsDTO>> { }

public class VersionsDTO
{
    public PlatformDTO Platform { get; set; }
    public string MinimumRequiredVersion { get; set; } = default!;
    public string CurrentlySupportedVersion { get; set; } = default!;
}
```
