# Creating an aggregate

## Intro

Aggregates, as defined by Domain Driven Design, are clusters of related objects which can be treated as a single domain entity. One of these objects is distinguished as an aggregate root. Every object of an aggregate must be accessed through the aggregate root. This guide will present how to create a simple aggregate using LeanCode CoreLibrary.

## Scenario

Let's imagine a simple project management app where employees can create projects which contain assignments to which people can be assigned. We will create two aggregates - one representing a project and the other representing a person which can be assigned to a assignment.

## Aggregates

Let's start with a simple model for the project aggregate.

```csharp
public class Project : IAggregateRoot<SId<Project>>
{
    private readonly List<Assignment> assignments = new();

    public SId<Project> Id { get; private init; }
    public string Name { get; private set; } = default!;

    public IReadOnlyList<Assignment> Assignments => assignments;

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Project() { }

    public static Project Create(string name)
    {
        return new Project { Id = SId<Project>.New(), Name = name, };
    }
}
```

As you can see, the class implements `IAggregateRoot` interface - it marks the class as being the root of an aggregate. Moreover the `Id` field of the class is of type `SId<Project>` - it is a special type present in the CoreLibrary which is basically a Guid prefixed with a class name. In this case, the Id of the Project will look somewhat like `project_45a8f39f-9df0-4a23-b781-2a46de22fac1`.
The `Project` also has a list of `Assignments`. Notice that there are two lists containing assignments of a project - the `Assignments` one is a readonly interface for the `assignments` which contents can be changed by the class. Generally, we try to model the API in such a way that the objects cannot be changed from the outside - an object's state should be modified only by the methods it exposes.

Let's follow with a class representing an assignment belonging to a project:

```csharp
public class Assignment : IIdentifiable<SId<Assignment>>
{
    public SId<Assignment> Id { get; private init; }
    public string Name { get; private set; } = default!;
    public AssignmentStatus Status { get; private set; }
    public SId<Employee>? AssignedEmployeeId { get; private set; }

    public SId<Project> ParentProjectId { get; private init; } = default!;
    public Project ParentProject { get; private init; } = default!;

    private Assignment() { }

    public static Assignment Create(Project parentProject, string name)
    {
        return new Assignment
        {
            Id = SId<Assignment>.New(),
            Name = name,
            ParentProject = parentProject,
            ParentProjectId = parentProject.Id,
            Status = AssignmentStatus.NotStarted,
        };
    }

    public enum AssignmentStatus
    {
        NotStarted,
        InProgress,
        Finished,
    }
}
```

Notice that an assignment is not an aggregate root - it can only be accessed through the project to which it belongs.

Likewise, let's create a class representing an employee which can be assigned to an assignment:

```csharp
public class Employee : IAggregateRoot<SId<Employee>>
{
    public SId<Employee> Id { get; private init; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Employee() { }

    public static Employee Create(string name, string email)
    {
        return new Employee
        {
            Id = SId<Employee>.New(),
            Name = name,
            Email = email,
        };
    }
}
```

Notice that the `Employee` class is an aggregate root - the employee represents a standalone business domain object with its own meaning and behavior.

## Adding logic

So far, only the structure of objects has been defined but business domain objects usually has some set of behaviors which they can execute. In this section we will see how a logic can be added to the aggregates.

Let's start with a simple example of adding a few methods to the `Assignment` class:

```csharp
public class Assignment : IIdentifiable<SId<Assignment>>
{
    . . .

    public void Edit(string name)
    {
        Name = name;
    }

    public void AssignEmployee(SId<Employee> employeeId)
    {
        AssignedEmployeeId = employeeId;
    }

    public void UnassignEmployee()
    {
        AssignedEmployeeId = null;
    }

    public void ChangeStatus(AssignmentStatus status)
    {
        Status = status;
    }
}
```

Remember that the only way to access an assignment is to go through the relevant project which is the aggregate root. Because of this we will add some methods to the `Project` class which will allow us to interact with the assignments:

```csharp
public class Project : IAggregateRoot<SId<Project>>
{
    . . .

    public void AddAssignments(IEnumerable<string> assignmentNames)
    {
        this.assignments.AddRange(assignmentNames.Select(tn => Assignment.Create(this, tn)));
    }

    public void EditTask(SId<Assignment> assignmentId, string name)
    {
        assignments.Single(t => t.Id == assignmentId).Edit(name);
    }

    public void AssignEmployeeToAssignment(SId<Assignment> assignmentId, SId<Employee> employeeId)
    {
        assignments.Single(t => t.Id == assignmentId).AssignEmployee(employeeId);
        DomainEvents.Raise(new UserAssignedToAssignment(assignmentId, employeeId));
    }

    public void UnassignEmployeeFromAssignment(SId<Assignment> assignmentId)
    {
        assignments.Single(t => t.Id == assignmentId).UnassignEmployee();
    }

    public void ChangeAssignmentStatus(SId<Assignment> assignmentId, Assignment.AssignmentStatus status)
    {
        assignments.Single(t => t.Id == assignmentId).ChangeStatus(status);
    }
}
```

Notice that these added methods can and will throw an exception if a project does not contain any assignment with provided Id. This is an excepted behavior - checks for respecting domain logic should be performed in respective command (or operation) validators.

## Rasing domain events

The domain events make an important part of DDD. It is through the use of domain events that an aggregate may communicate with the rest of the system. In this section we will see how to raise an event from an aggregate.

Let's imagine that we want to perform some action after an employee has been assigned to an assignment. We will modify `AssignEmployeeToAssignment` method from the `Project` class:

```csharp
public class Project : IAggregateRoot<SId<Project>>
{
    . . .

    public void AssignEmployeeToAssignment(SId<Assignment> assignmentId, SId<Employee> employeeId)
    {
        assignments.Single(t => t.Id == assignmentId).AssignEmployee(employeeId);
        DomainEvents.Raise(new EmployeeAssignedToAssignment(assignmentId, employeeId));
    }
}
```

`EmployeeAssignedToAssignment` has to implement `IDomainEvent` interface. After being raised, the event can be handled by the matching `IConsumer` to perform wanted action. To read more about events, see [handling events](./0X_handling_events). <!-- TODO: add final name of the handling events file -->
