# Domain event

In Domain-Driven Design, a domain event is a pattern used to represent a significant change or occurrence within a domain. It is a way of capturing and communicating an event that has happened in the system and might be of interest to other parts of the system.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.DomainModels | [![NuGet version (LeanCode.DomainModels)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.svg?style=flat-square)](https://www.nuget.org/packages/LeanCode.DomainModels/8.0.2260-preview/) | `IAggregateRoot`, `IDomainEvent` |
| LeanCode.DomainModels.Generators | [![NuGet version (LeanCode.DomainModels.Generators)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.Generators.svg?style=flat-square)](https://www.nuget.org/packages/LeanCode.DomainModels.Generators/8.0.2260-preview/) | Ids |

## Example

Let's imagine that we want to perform some action after a employee has been assigned to a assignment. We will modify `AssignEmployeeToAssignment` method from the `Project` class:

```csharp
public class Project : IAggregateRoot<ProjectId>
{
    // . . .

    public void AssignEmployeeToAssignment(
        AssignmentId assignmentId,
        EmployeeId employeeId)
    {
        assignments.Single(t => t.Id == assignmentId)
            .AssignEmployee(employeeId);

        DomainEvents.Raise(new EmployeeAssignedToAssignment(
            AssignmentId assignmentId,
            EmployeeId employeeId));
    }
}
```

```csharp
public class EmployeeAssignedToAssignment : IDomainEvent
{
    public Guid Id { get; private init; }
    public DateTime DateOccurred { get; private init; }

    public AssignmentId AssignmentId { get; private init; }
    public EmployeeId EmployeeId { get; private init; }

    public EmployeeAssignedToAssignment(
        AssignmentId assignmentId,
        EmployeeId employeeId)
    {
        Id = Guid.NewGuid();
        DateOccurred = Time.UtcNow;

        AssignmentId = assignmentId;
        EmployeeId = employeeId;
    }

    [JsonConstructor]
    public EmployeeAssignedToAssignment(
        Guid id,
        DateTime dateOccurred,
        AssignmentId assignmentId,
        EmployeeId employeeId
    )
    {
        Id = id;
        DateOccurred = dateOccurred;
        AssignmentId = assignmentId;
        EmployeeId = employeeId;
    }
}
```

Ensure that `EmployeeAssignedToAssignment` implements the `IDomainEvent` interface, and it has a constructor with the `[JsonConstructor]` attribute. `[JsonConstructor]` attribute is used during JSON deserialization to specify which constructor should be used to create an object from JSON data. This attribute ensures that when a message is deserialized from JSON, the correct constructor is invoked to create the object.

After being raised, the event can be handled by the matching `IConsumer` to perform wanted action.

> **Tip:** To read how to handle domain events, visit [here](../../external_integrations/messaging_masstransit/handling_events.md).
