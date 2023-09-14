# Creating an aggregate

## Intro

Aggregates, as defined by Domain Driven Design, are clusters of related objects which can be treated as a single domain entity. One of these objects is distinguished as an aggregate root. Every object of an aggregate must be accessed through the aggregate root. This guide will present how to create a simple aggregate using the LeanCode CoreLibrary.

## Scenario

Let's imagine a simple project management app where users can create projects which contain tasks to which people can be assigned. We will create two aggregates - one representing a project and the other representing a person which can be assigned to the task.

## Aggregates

Let's start with a simple model for the project aggregate.

```csharp
[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "project")]
public readonly partial record struct ProjectId;

public class Project : IAggregateRoot<ProjectId>
{
    private readonly List<Task> tasks = new();

    public ProjectId Id { get; private init; }
    public string Name { get; private set; }

    public IReadOnlyList<Task> Tasks => tasks;

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Project() { }

    public static Project Create(string name)
    {
        return new Project
        {
            Id = ProjectId.New(),
            Name = name,
        };
    }
}
```

As you can see, the class implements `IAggregateRoot` interface - it marks the class as being the root of an aggregate. Moreover the `Id` field of the class is of type `ProjectId` - it is a special source-generated type present in the CoreLibrary. You can read more about `Id` types [here](../../domain/ids.md). In this case, the Id of the Project will look somewhat like `project_45a8f39f-9df0-4a23-b781-2a46de22fac1`.
The `Project` also has a list of `Tasks`. Notice that there are two lists containing tasks of a project - the `Tasks` one is a readonly interface for the `tasks` which contents can be changed by the class. Generally, we try to model the API in such a way that the objects cannot be changed from the outside - an object's state should be modified only by the methods it exposes.

Let's follow with a class representing a task belonging to a project:

```csharp
[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "task")]
public readonly partial record struct TaskId;

public class Task : IIdentifiable<TaskId>
{
    public TaskId Id { get; private init; }
    public string Name { get; private set; }
    public TaskStatus Status { get; private set; }
    public UserId? AssignedUser { get; private set; }

    public Project ParentProject { get; private init; }

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Task() { }

    public static Task Create(Project parentProject, string name)
    {
        return new Task
        {
            Name = name,
            ParentProject = parentProject,
            Status = TaskStatus.NotStarted,
        };
    }

    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Finished,
    }
}
```

Notice that a task is not an aggregate root - it can only be accessed through the project to which it belongs.

Likewise, let's create a class representing a user which can be assigned to a task:

```csharp
[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "User")]
public readonly partial record struct UserId;

public class User : IAggregateRoot<UserId>
{
    public UserId Id { get; private init; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private User() { }

    public static User Create(string name, string email)
    {
        return new User
        {
            Id = UserId.New(),
            Name = name,
            Email = email,
        };
    }
}
```

Notice that the `User` class is an aggregate root - the user represents a standalone business domain object with its own meaning and behavior.

## Adding logic

So far, only the structure of objects has been defined but business domain objects usually has some set of behaviors which they can execute. In this section we will see how a logic can be added to the aggregates.

Let's start with a simple example of adding a few methods to the `Task` class:

```csharp
public class Task : IIdentifiable<TaskId>
{
    . . .

    public void Edit(string name)
    {
        Name = name;
    }

    public void AssignUser(UserId userId)
    {
        AssignedUser = userId;
    }

    public void UnassignUser()
    {
        AssignedUser = null;
    }

    public void ChangeStatus(TaskStatus status)
    {
        Status = status;
    }
}
```

Remember that the only way to access a task is to go through the relevant project which is the aggregate root. Because of this we will add some methods to the `Project` class which will allow us to interact with the tasks:

```csharp
public class Project : IAggregateRoot<ProjectId>
{
    . . .

    public void AddTasks(IEnumerable<string> taskNames)
    {
        this.tasks.AddRange(taskNames.Select(tn => Task.Create(this, tn)));
    }

    public void EditTask(TaskId taskId, string name)
    {
        tasks.Single(t => t.Id == taskId).Edit(name);
    }

    public void AssignUserToTask(TaskId taskId, UserId userId)
    {
        tasks.Single(t => t.Id == taskId).AssignUser(userId);
    }

    public void UnassignUserFromTask(TaskId taskId)
    {
        tasks.Single(t => t.Id == taskId).UnassignUser(userId);
    }

    public void ChangeTaskStatus(TaskId taskId, TaskStatus status)
    {
        tasks.Single(t => t.Id == taskId).ChangeTaskStatus(status);
    }
}
```

Notice that these added methods can and will throw an exception if a project does not contain any task with provided Id. This is an excepted behavior - checks for respecting domain logic should be performed in respective command (or operations) validators.

## Raising domain events

The domain events make an important part of DDD. It is through the use of domain events that an aggregate may communicate with the rest of the system. In this section we will see how to raise an event from an aggregate.

Let's imagine that we want to perform some action after a user has been assigned to a task. We will modify `AssignUserToTask` method from the `Project` class:

```csharp
public class Project : IAggregateRoot<ProjectId>
{
    . . .

    public void AssignUserToTask(TaskId taskId, UserId userId)
    {
        tasks.Single(t => t.Id == taskId).AssignUser(userId);
        DomainEvents.Raise(new UserAssignedToTask(TaskId taskId, UserId userId));
    }
}
```

`UserAssignedToTask` has to implement `IDomainEvent` interface. After being raised, the event can be handled by the matching `IConsumer` to perform wanted action. To read more about events, see [handling events](./handling_events.md). <!-- TODO: add final name of the handling events file -->
