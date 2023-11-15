# Feature flags - ConfigCat

ConfigCat is a feature flag service that allows to toggle features on or off after deploying code. It offers support for percentage rollouts, A/B testing, and feature variations. By integrating ConfigCat with the LeanCode CoreLibrary, you can effortlessly harness its feature-flag capabilities. For in-depth information about ConfigCat, explore the [official documentation](https://configcat.com/docs/) and the [.NET SDK documentation](https://configcat.com/docs/sdk-reference/dotnet/).

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| ConfigCat.Client | [![NuGet version (ConfigCat.Client)](https://img.shields.io/nuget/v/ConfigCat.Client.svg?style=flat-square)](https://www.nuget.org/packages/ConfigCat.Client/8.2.0/) | Client |
| LeanCode.ConfigCat | [![NuGet version (LeanCode.ConifgCat)](https://img.shields.io/nuget/vpre/LeanCode.ConfigCat.svg?style=flat-square)](https://www.nuget.org/packages/LeanCode.ConfigCat/8.0.2260-preview/) | Configuration |

## Configuration

To incorporate ConfigCat into your LeanCode CoreLibrary-based application, follow the example below. This example showcases the usage of the `AddConfigCat(...)` method to register the `ConfigCatClient`:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // . . .

    services.AddConfigCat(
        sdkKey: null,
        flagOverridesFilePath: "YOUR_PATH/configcat.json",
        overrideBehaviour: OverrideBehaviour.LocalOnly);

    // . . .
}
```

Then you can define `configcat.json` file:

```json
{
    "flags": {
        "SendEmployeeAssignedToAssignmentEmails": false,
    }
}
```

In the example above, ConfigCat is configured to retrieve feature flags directly from a `configcat.json` file specified in the `flagOverridesFilePath` argument, bypassing the ConfigCat CDN by using the `OverrideBehaviour.LocalOnly` setting. If you want to fetch values from the ConfigCat CDN, you must set the `sdkKey` and modify the `overrideBehaviour` argument with one of the following options:

```csharp
public enum OverrideBehaviour
{
    //
    // Summary:
    //     When evaluating values, the SDK will not use feature
    //     flags and settings from the ConfigCat CDN, but it will
    //     use all feature flags and settings that are loaded from
    //     local-override sources.
    LocalOnly,
    //
    // Summary:
    //     When evaluating values, the SDK will use all feature
    //     flags and settings that are downloaded from the ConfigCat CDN,
    //     plus all feature flags and settings that are loaded from
    //     local-override sources. If a feature flag or a setting
    //     is defined both in the fetched and the local-override
    //     source then the local-override version will take precedence.
    LocalOverRemote,
    //
    // Summary:
    //     When evaluating values, the SDK will use all feature flags
    //     and settings that are downloaded from the ConfigCat CDN, plus
    //     all feature flags and settings that are loaded from local-override
    //     sources. If a feature flag or a setting is defined both in the
    //     fetched and the local-override source then the fetched version
    //     will take precedence.
    RemoteOverLocal
}
```

You can also explore the various [IServiceCollection ConfigCat extensions](https://github.com/leancodepl/corelibrary/blob/v8.0-preview/src/Infrastructure/LeanCode.ConfigCat/ConfigCatExtensions.cs) to register ConfigCat according to your preferences.

## Feature flags usage

After configuration, you can use feature flag defined above to modify behaviour of an application to not send `EmployeeAssignedToAssignmentEmail` email when employee has been assigned to assignment:

```csharp
public class SendEmailToEmployeeOnEmployeeAssignedToAssignment
    : IConsumer<EmployeeAssignedToAssignment>
{
    private readonly IEmailSender emailSender;
    private readonly IRepository<Employee, EmployeeId> employees;
    private readonly IRepository<Assignment, AssignmentId> assignments;
    private readonly IConfigCatClient configCat;

    public SendEmailToEmployeeOnEmployeeAssignedToAssignment(
        IEmailSender emailSender,
        IRepository<Employee, EmployeeId> employees,
        IRepository<Assignment, AssignmentId> assignments,
        IConfigCatClient configCat)
    {
        this.emailSender = emailSender;
        this.employees = employees;
        this.assignments = assignments;
        this.configCat = configCat;
    }

    public async Task Consume(
        ConsumeContext<EmployeeAssignedToAssignment> context)
    {
        if (await configCat.GetValueAsync(
                key: "SendEmployeeAssignedToAssignmentEmails",
                defaultValue: false,
                cancellationToken: context.CancellationToken))
        {
            var msg = context.Message;

            var employee = await employees.FindAndEnsureExistsAsync(
                msg.EmployeeId,
                context.CancellationToken);

            var assignment = await assignments.FindAndEnsureExistsAsync(
                msg.AssignmentId,
                context.CancellationToken);

            await emailSender.SendEmployeeAssignedToAssignmentEmailAsync(
                employee,
                assignment,
                context.CancellationToken);
        }
    }
}
```
