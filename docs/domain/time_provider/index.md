# Time provider

`LeanCode.TimeProvider` is utilized to centralize and manage time-related operations. It enables the domain to encapsulate temporal logic, ensuring consistency and testability. Employing a dedicated time provider, facilitates easier maintenance and testing of time-dependent domain logic.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.DomainModels | [![NuGet version (LeanCode.DomainModels)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.DomainModels) | `IAggregateRoot` |
| LeanCode.TimeProvider | [![NuGet version (LeanCode.TimeProvider)](https://img.shields.io/nuget/vpre/LeanCode.TimeProvider.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.TimeProvider) | `Time` |

## Example

Let's enhance our project aggregate by introducing a `DateCreated` property. This property will hold the timestamp indicating the UTC time when the project was originally created:

```csharp
public class Project : IAggregateRoot<ProjectId>
{
    // . . .

    DateTime DateCreated { get; private set; }

    // . . .

    public static Project Create(
        ProjectId projectId,
        string name,
        EmployeeId ownerId)
    {
        return new Project
        {
            Id = projectId,
            Name = name,
            OwnerId = ownerId,
            DateCreated = Time.UtcNow,
        };
    }
}
```

!!! tip
    Employing a single instance of `LeanCode.TimeProvider.Time` throughout the domain enables convenient manipulation of time for testing purposes. Further information about this can be found [here](../../tests/faking_time/index.md).
