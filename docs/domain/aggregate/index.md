# Aggregate

Aggregates, as defined by Domain Driven Design, are clusters of related objects which can be treated as a single domain entity. One of these objects is distinguished as an aggregate root. Every object of an aggregate must be accessed through the aggregate root. Transactions should not cross aggregate boundaries.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.DomainModels | [![NuGet version (LeanCode.DomainModels)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.DomainModels) | `IAggregateRoot` |
| LeanCode.DomainModels.Generators | [![NuGet version (LeanCode.DomainModels.Generators)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.Generators.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.DomainModels.Generators) | Ids |

## Scenario

Let's imagine a simple project management app where employees can create projects which contain assignments to which people can be assigned. We will create two aggregates - one representing a project and the other representing a person which can be assigned to the assignment.

## Example

### Project

Let's define a simple model for the project aggregate:

```csharp
[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "project")]
public readonly partial record struct ProjectId;

public class Project : IAggregateRoot<ProjectId>
{
    private readonly List<Assignment> assignments = new();

    public ProjectId Id { get; private init; }
    public EmployeeId OwnerId { get; private init; }
    public string Name { get; private set; }

    public IReadOnlyList<Assignment> Assignments => assignments;

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Project() { }

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
        };
    }
}
```

As you can see, the class implements `IAggregateRoot` interface - it marks the class as being the root of an aggregate. Moreover the `Id` field of the class is of type `ProjectId` - it is a special source-generated type present in the CoreLibrary. You can read more about `Id` types [here](../id/index.md). In this case, the Id of the project will look somewhat like `project_45a8f39f9df04a23b7812a46de22fac1`.
The Project also has a list of `Assignments`. Notice that there are two lists containing assignments of a project - the `Assignments` one is a readonly interface for the `assignments` which contents can be changed by the class. Moreover project has `OwnerId` property which is an Id of employee who created the project. Generally, we try to model the API in such a way that the objects cannot be changed from the outside - an object's state should be modified only by the methods it exposes.

### Employee

Likewise, let's create a class representing a employee which can be assigned to a assignment:

```csharp
[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "employee")]
public readonly partial record struct EmployeeId;

public class Employee : IAggregateRoot<EmployeeId>
{
    public EmployeeId Id { get; private init; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Employee() { }

    public static Employee Create(string name, string email)
    {
        return new Employee
        {
            Id = EmployeeId.New(),
            Name = name,
            Email = email,
        };
    }
}
```

Notice that the `Employee` class is an aggregate root - the employee represents a standalone business domain object with its own meaning and behavior.

### Adding logic

So far, only the structure of objects has been defined but business domain objects usually has some set of behaviors which they can execute.

Let's add a few methods to the `Project` class:

```csharp
public class Project : IAggregateRoot<ProjectId>
{
    // . . .

    public void UpdateName(string name)
    {
        Name = name;
    }

    public void AddAssignments(IEnumerable<string> assignmentNames)
    {
        assignments.AddRange(
            assignmentNames.Select(tn => Assignment.Create(this, tn)));
    }

    public void EditAssignment(AssignmentId assignmentId, string name)
    {
        assignments.Single(t => t.Id == assignmentId)
            .Edit(name);
    }

    public void AssignEmployeeToAssignment(
        AssignmentId assignmentId,
        EmployeeId employeeId)
    {
        assignments.Single(t => t.Id == assignmentId)
            .AssignEmployee(employeeId);
    }

    public void UnassignEmployeeFromAssignment(AssignmentId assignmentId)
    {
        assignments.Single(t => t.Id == assignmentId)
            .UnassignEmployee(employeeId);
    }

    public void ChangeAssignmentStatus(
        AssignmentId assignmentId,
        AssignmentStatus status)
    {
        assignments.Single(t => t.Id == assignmentId)
            .ChangeAssignmentStatus(status);
    }
}
```

!!! tip
    To see `Assignment` class implementation visit [here](../entity/index.md).

Notice that these added methods can and will throw an exception if a project does not contain any assignment with provided Id. This is an excepted behavior - checks for respecting domain logic should be performed in respective command (or operations) validators.
