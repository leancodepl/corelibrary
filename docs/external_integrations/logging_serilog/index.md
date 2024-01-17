# Logging - Serilog

Efficiently managing logs is an essential element in maintaining the health and performance of any software system. Serilog, integrated into the LeanCode CoreLibrary, offers a logging solution that empowers developers with insights into application behavior. This integration enables easy configuration and customization of logging. For further details and guidance on implementing Serilog, explore the official [Serilog](https://serilog.net/) documentation and the [Serilog GitHub repository](https://github.com/serilog/serilog).

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.Logging | [![NuGet version (LeanCode.Logging)](https://img.shields.io/nuget/vpre/LeanCode.SendGrid.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.Logging) | Configuration |
| Serilog | [![NuGet version (LeanCode.Logging)](https://img.shields.io/nuget/v/Serilog.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Serilog) | Logging |

## Configuration

To incorporate Serilog into your LeanCode CoreLibrary-based application, follow the example below. Customize the minimum log level by modifying the `Logging:MinimumLevel` envirnoment variable (explore possible log levels [here](https://github.com/serilog/serilog/wiki/Configuration-Basics#minimum-level)), activate detailed internal logs via `Logging:EnableDetailedInternalLogs`, configure `LoggerFilterOptions` using the `Serilog:SystemLoggers` section, and specify `Logging:SeqEndpoint` to direct logs to [Seq]. The provided configuration below defaults to writing logs to the console and, if `Logging:SeqEndpoint` is provided, also to [Seq]. The `ConfigureDefaultLogging` extension method on `IHostBuilder` is used to set up logging.

```sh
export Logging__EnableDetailedInternalLogs=true
export Logging__MinimumLevel=Verbose
```

```csharp
public class Program
{
    public static Task Main() => CreateWebHostBuilder().Build().RunAsync();

    public static IHostBuilder CreateWebHostBuilder()
    {
        // The `BuildMinimalHost` method includes environment
        // variables as part of the configuration and configures
        // Kestrel as the web server.
        return LeanProgram
            .BuildMinimalHost<Startup>()
            .ConfigureDefaultLogging(
                "ExampleApp.Api",
                new[] { typeof(Program).Assembly });
    }
}
```

!!! tip
    If you rely on `appsettings.json` for your configuration or intend to use different configurations within your `Program.cs`, you can simply invoke `ConfigureDefaultLogging` on your `IHostBuilder`.

Once configured, leverage Serilog for logging purposes in your application:

```csharp
public class UpdateProjectNameCH : ICommandHandler<UpdateProjectName>
{
    private readonly Serilog.ILogger logger =
        Serilog.Log.ForContext<UpdateProjectNameCH>();

    // . . .

    public Task ExecuteAsync(HttpContext context, UpdateProjectName command)
    {
        // . . .

        logger.Information(
            "Project's {ProjectId} name has been updated",
            project.Id);
    }
}
```

[Seq]: https://datalust.co/seq
