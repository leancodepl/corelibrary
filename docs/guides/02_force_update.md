# Force update

## Configuration

To enforce or suggest updates for client apps, you can utilize the `AddForceUpdate(...)` extension method from the `LeanCode.ForceUpdate` package in the `Startup.cs` file. This method is available on the `IServiceCollection` and needs to be called after `AddCQRS(...)`. The following example demonstrates the usage:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddCQRS(CQRSTypes, CQRSTypes)
            .AddForceUpdate(
                new Version(AndroidMinimumRequiredVersion),
                new Version(AndroidCurrentlySupportedVersion),
                new Version(IOSMinimumRequiredVersion),
                new Version(IOSCurrentlySupportedVersion));
    // ...
}
```

## Version support
After configuation above. `VersionSupport` query is created and available at `/cqrs/query/LeanCode.ForceUpdate.Contracts.VersionSupport`.
```csharp
public class VersionSupport : IQuery<VersionSupportDTO>
{
    public PlatformDTO Platform { get; set; }
    public string Version { get; set; }
}

public class VersionSupportDTO
{
    public string CurrentlySupportedVersion { get; set; }
    public string MinimumRequiredVersion { get; set; }
    public VersionSupportResultDTO Result { get; set; }
}

public enum PlatformDTO
{
    Android = 0,
    IOS = 1,
}

public enum VersionSupportResultDTO
{
    UpdateRequired = 0,
    UpdateSuggested = 1,
    UpToDate = 2,
}
```
The query takes `Platform` (either IOS or Android) and the `Version` of the client app as parameters. It returns whether the client's version is supported, `CurrentlySupportedVersion` and `MinimumRequiredVersion` for specified provider. If the client's version is below the `MinimumRequiredVersion`, the response will indicate that an update is needed. If the client's version is greater than or equal to the `MinimumRequiredVersion` and less than the `CurrentlySupportedVersion`, the response will suggest an update. If the app version is greater than or equal to the `CurrentlySupportedVersion`, the response will indicate that the app is up to date. It's also possible to override this behavior by overriding the `CheckVersion` method in the `VersionHandler` class, responsible for version checking:

```csharp
public class VersionHandler
{
    // ...
    public virtual Task<VersionSupportResult> CheckVersionAsync(
        Version version,
        Platform platform,
        HttpContext context
    )
    {
        var (minimumRequiredVersion, currentlySupportedVersion) = platform switch
        {
            Platform.Android => (androidConfiguration.MinimumRequiredVersion, androidConfiguration.CurrentlySupportedVersion),
            Platform.IOS => (iOSConfiguration.MinimumRequiredVersion, iOSConfiguration.CurrentlySupportedVersion),
            _ => throw new InvalidOperationException($"Invalid platform: {platform}"),
        };

        if (version < minimumRequiredVersion)
        {
            return Task.FromResult(VersionSupportResult.UpdateRequired);
        }
        else if (version >= minimumRequiredVersion && version < currentlySupportedVersion)
        {
            return Task.FromResult(VersionSupportResult.UpdateSuggested);
        }
        else
        {
            return Task.FromResult(VersionSupportResult.UpToDate);
        }
    }
}

public enum Platform
{
    Android,
    IOS,
}

public enum VersionSupportResult
{
    UpdateRequired,
    UpdateSuggested,
    UpToDate,
}
```
Below is an example of overriding the `CheckVersion` method:
```csharp
public class CustomVersionHandler : VersionHandler
{
    public override Task<VersionSupportResult> CheckVersionAsync(
        Version version,
        Platform platform,
        HttpContext context
    )
    {
    // You can also use for example UserId from HttpContext
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
