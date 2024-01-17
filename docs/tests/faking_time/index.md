# Faking time

When conducting tests involving time-related functionalities, manipulating time using fake providers helps in verifying the behavior of your code under various time-related conditions, without relying on the actual system time.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.TimeProvider.TestHelpers | [![NuGet version (LeanCode.TimeProvider.TestHelpers)](https://img.shields.io/nuget/vpre/LeanCode.TimeProvider.TestHelpers.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.TimeProvider.TestHelpers) | `TestTimeProvider` |

## Example

The following example demonstrates the usage of `TestTimeProvider.ActivateFake` to establish a custom time provider within the context of `LeanCode.TimeProvider.Time`. This mechanism enables control over the time returned by the system. In this scenario, a new project, as defined [here](../../domain/time_provider/index.md), is instantiated. By reviewing the time obtained from `DateCreated` property against the anticipated UTC time, this test ensures that the manipulation of time aligns accurately with the intended functionality.

```csharp
[Fact]
public void Activate_fake_fakes_time()
{
    var expectedTime = new DateTimeOffset(
        new DateTime(2016, 11, 30, 0, 0, 0, DateTimeKind.Utc));

    // Activate the fake time with the expected time
    // and no time zone information.
    TestTimeProvider.ActivateFake(expectedTime, timeZoneInfo: null);

    var project = Project.Create(
        ProjectId.New(),
        "Project name",
        EmployeeId.New());

    // Assert that the `DateCreated` time matches
    // the expected time in UTC.
    Assert.Equal(expectedTime.UtcDateTime, project.DateCreated);
}
```
