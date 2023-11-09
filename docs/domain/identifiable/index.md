# Identifiable

In Domain-Driven Design, entity (marked by `IIdentifiable` in LeanCode CoreLibrary) is a concept used to model a distinct and identifiable object within the domain that is defined by its characteristics and identity. Entities are objects that have a distinct lifecycle and are distinguishable from other objects based on their unique identity.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.DomainModels | [![NuGet version (LeanCode.DomainModels)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.svg?style=flat-square)](https://www.nuget.org/packages/LeanCode.DomainModels/8.0.2260-preview/) | `IIdentifiable` |
| LeanCode.DomainModels.Generators | [![NuGet version (LeanCode.DomainModels.Generators)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.Generators.svg?style=flat-square)](https://www.nuget.org/packages/LeanCode.DomainModels.Generators/8.0.2260-preview/) | Ids |

## Example

Let's define a class representing a assignment belonging to a project:

```csharp
[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "assignment")]
public readonly partial record struct AssignmentId;

public class Assignment : IIdentifiable<AssignmentId>
{
    public AssignmentId Id { get; private init; }
    public string Name { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public EmployeeId? AssignedEmployee { get; private set; }

    public Project ParentProject { get; private init; }

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Assignment() { }

    public static Assignment Create(Project parentProject, string name)
    {
        return new Assignment
        {
            Name = name,
            ParentProject = parentProject,
            Status = AssignmentStatus.NotStarted,
        };
    }

    public void Edit(string name)
    {
        Name = name;
    }

    public void AssignEmployee(EmployeeId employeeId)
    {
        AssignedEmployee = employeeId;
    }

    public void UnassignEmployee()
    {
        AssignedEmployee = null;
    }

    public void ChangeStatus(AssignmentStatus status)
    {
        Status = status;
    }

    public enum AssignmentStatus
    {
        NotStarted,
        InProgress,
        Finished,
    }
}
```

Notice that `Assignment` is not an aggregate root - it can only be accessed through the `Project` to which it belongs. Methods from the `Assignment` class should be executed within the context of the `Project` aggregate. This aligns with the principle that an aggregate should function as a singular unit of transaction, ensuring that modifications to a `Assignment` object occur exclusively within the context of a corresponding `Project`. Any attempt to modify a `Assignment` object without the associated `Project` context should be restricted to maintain data integrity.
