# Writing commands

## Intro

Interacting with systems that are based on the CoreLibrary follows the Command Query Responsibility Segregation ([CQRS](../../basics/cqrs.md)) principle. As such, there are two main types of actions which can be performed in the system: commands, and [queries](./writing_queries.md) [^1]. In this guide we will focus on the former. A command is used to change the state of the system but it does not yield any results other then whether it succeeded — it can be treated as write-only action.

Each command consists of three parts:

* Contract
* (Optional) Command validator
* Command handler

The validation is performed separately from the execution of the command. The reason is adherence to the single responsibility principle. Moreover, handler implementation is made easier, both to write and read, due to the fact that it contains only the actual business process. Also, the data received from a client is passed from validator to handler implicitly.

## Contract

A contract, as the name suggests, defines the way a command can be invoked by the clients. Let's see how a contract for creating a project might look like.

```csharp
[AllowUnauthorized]
public class CreateProject : ICommand
{
    public string Name { get; set; }

    public static class ErrorCodes
    {
        public const int NameCannotBeEmpty = 1;
        public const int NameTooLong = 2;
    }
}
```

The contract for a command is simply a class implementing the `ICommand` interface. The fields of a contract class define how the body of an HTTP request should look like. In this case, an example of a valid request body would be:

```json
{
    "Name": "abc",
}
```

Notice the `ErrorCodes` class. These define possible validation errors which can be expected by the clients. Each error code should be a unique (in a given contract) integer.

Writing contracts in that way makes it possible for web and mobile clients to generate their own version of contract classes with use of the [Contract Generator](https://github.com/leancodepl/contractsgenerator), which is also a part of the CoreLibrary.

Also, a contract can have authorization attributes, which define rules that have to be satisfied for a client to be able to invoke the command. Authorization, however, is an extension to the contracts system and not an integral part of it. To learn more about authorizers, see [authorization](./authorization.md).

## Command validator

Command validator's main role is to ensure that a command invoked by a client is valid. Command validator should check the structure of the command itself, for example, check for required fields, check whether passed strings do not exceed maximum length, etc. Also the validator should check business requirements of the command. For example, when editing a project, it should check whether or not the project exists. See [more complex commands](#more-complex-commands) for an example. Let's see how a validator for `CreateProject` command might look like:

```csharp
public class CreateProjectCV : Validator<CreateProject>
{
    public CreateProjectCV()
    {
        RuleFor(cmd => cmd.Name)
            .NotEmpty()
            .WithCode(CreateProject.ErrorCodes.NameCannotBeEmpty)
            .MaximumLength(500)
            .WithCode(CreateProject.ErrorCodes.NameTooLong);
    }
}
```

A validator for a command should inherit from `Validator<T>` where `T` is a type of command contract. The validation in the CoreLibrary builds upon [FluentValidation](https://docs.fluentvalidation.net/en/latest/), adding `WithCode()` method, which is responsible for adding an error code to the response in case the validation fails. In the above example, trying to change a project name to one which is over 500 characters long would cause a command to fail and return an appropriate error code to the client.

## Command handler

!!! note
    A command validator and handler are usually put in a single file with a `CH` suffix, e.g. `CreateProjectCH.cs`.

Finally, we arrive at a command handler that is the part which actually executes the command. For the `CreateProject` command, a handler may look like this:

```csharp
public class CreateProjectCH : ICommandHandler<CoreContext, CreateProject>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CreateProjectCH>();

    private readonly IRepository<Project, SId<Project>> projects;

    public CreateProjectCH(IRepository<Project, SId<Project>> projects)
    {
        this.projects = projects;
    }

    public Task ExecuteAsync(CoreContext context, CreateProject command)
    {
        var project = Project.Create(command.Name);
        projects.Add(project);

        logger.Information("Project {ProjectId} added", project.Id);
    }
}
```

!!! warning
    According to DDD rules, a single command should only modify a single aggregate as an aggregate should be a transaction boundary in the system. If there is a need to modify multiple aggregates as a part of a single operation, raise events and modify all aggregates one by one in event handlers.

The logic of the handler is straightforward — a new project with a given name is created and added to `projects` repository. The `IRepository` interface is a part of the CoreLibrary which allows to abstract the logic of persisting domain entities and to avoid dependency on the project's `DbContext` which allows for easier unit testing without having to mock the `DbContext`. A repository (and other services) should be injected into a command handler using dependency injection. Also, notice that the `Add` method used to persist newly created project is synchronous. It doesn't commit anything to the database which makes it possible to use [transactional outbox](https://microservices.io/patterns/data/transactional-outbox.html) pattern. The actual asynchronous operation in the database is performed by `StoreAndPublishEvents()` element in the pipeline, see [pipeline](./pipeline.md) for more info. A basic repository for projects allowing for operations on database may look like this:

```csharp
public class ProjectsRepository<TContext>
    : EFRepository<Purchase, SId<Purchase>, TContext>
    where TContext : DbContext
{
    public ProjectsRepository(TContext dbContext)
        : base(dbContext) { }

    public override Task<Purchase?> FindAsync(SId<Purchase> id, CancellationToken cancellationToken = default)
    {
        return DbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)!;
    }
}
```

Here `EFRepository` is a class from the CoreLibrary containing default implementations for `Add`, `Delete` and `Update` methods which can be used to alter the contents of the database. Usually `FindAsync` method can be implemented as a simple retrieval of an entity based on Id but more complicated operations can also be done in the method if they are needed. Usually we don't wan to use `FindAsync` method of the corresponding `DbSet` to avoid concurrency errors resulting from entities being cached by the `DbSet`.

!!! warning
    If an implemented command does not work, make sure that all referenced types are properly registered in the dependency injection container.

## More complex commands

The command for creating a project is a really basic one — it only takes a name for new project and all of the validation can be done without checking the state of the domain. We will now show a slightly more complex command, which will be responsible for adding tasks to existing projects.

```csharp
[AllowUnauthorized]
public class AddTasksToProject : ICommand
{
    public string ProjectId { get; set; }
    public List<TaskDTO> Tasks { get; set; }

    public static class ErrorCodes
    {
        public const int ProjectDoesNotExist = 1;
        public const int TasksCannotBeNull = 2;
        public const int TasksCannotBeEmpty = 3;

        public sealed class Tasks : TaskDTO.ErrorCodes { }
    }
}


public class TaskDTO
{
    public string Name {get; set; }

    public class ErrorCodes
    {
        public const int NameCannotBeEmpty = 101;
        public const int NameTooLong = 102;
    }
}
```

Notice the use of `TaskDTO` class. DTO classes can be defined as a part of contracts and matching classes will be generated for web and mobile clients. Also, notice that in `TaskDTO` we used error codes starting from 101 to avoid clashes with error codes in `AddTasksToProject`. Ensuring the uniqueness of error codes never had been a problem, so there does not exist any sophisticated mechanism for it. Now, let's move on to validating the command:

```csharp
public class AddTasksToProjectCV : Validator<AddTasksToProject>
{
    public AddTasksToProjectCV()
    {
        RuleFor(cmd => cmd.Tasks)
            .NotNull()
            .WithCode(AddTasksToProject.ErrorCodes.TasksCannotBeNull)
            .NotEmpty()
            .WithCode(AddTasksToProject.ErrorCodes.TasksCannotBeEmpty);

        RuleForEach(cmd => cmd.Tasks)
            .ChildRules(child => child.RuleFor(c => c).SetValidator(new TaskDTOValidator()));

        MustAsync(ProjectExistsAsync)
            .Equal(true)
            .WithCode(AddContactToSite.ErrorCodes.ProjectDoesNotExist)
            .WithMessage("A project with given Id does not exist.");
    }


    private static async Task<bool> ProjectExistsAsync(
        AddTasksToProject cmd,
        string projectId,
        PropertyValidatorContext ctx,
        CancellationToken cancellationToken
    )
    {
        if (!SId<Project>.TryParse(projectId, out var pid))
        {
            return false;
        }

        var dbContext = ctx.ParentContext.GetService<CoreDbContext>();

        return dbContext.Projects.AnyAsync(p => p.Id == pid, cancellationToken);
    }
}

public class TaskDTOValidator : AbstractValidator<TaskDTO>
{
    public TaskDTOValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithCode(TaskDTO.ErrorCodes.NameCannotBeEmpty)
            .MaximumLength(500)
            .WithCode(TaskDTO.ErrorCodes.NameTooLong);
    }
}
```

There are two important things to note here. First one is the use of a custom validator for validating `TaskDTO`. Such validators should inherit from FluentValidation's `AbstractValidator<T>` class and they allow for reusing the validation logic in commands using the same DTO classes. The second one is performing asynchronous validation — to check whether a project we want to add the tasks for exists, we perform a database query inside the `ProjectExistsAsync` method. This allows to check that the command to be executed does not violate the logic of the business domain.

Also, it is a good practice to chain the custom validation rules, `ProjectExistsAsync` in the above example, with a call to `WithMessage` function. This makes validation result more informative.

Now, let's implement the handler for the command:

```csharp
public class AddTasksToProjectCH : ICommandHandler<CoreContext, AddTasksToProject>
{

    private readonly IRepository<Project, SId<Project>> projects;

    public AddTasksToProjectCH(IRepository<Project, SId<Project>> projects)
    {
        this.projects = projects;
    }

    public async Task ExecuteAsync(CoreContext context, AddTasksToProject command)
    {
        var project = await projects.FindAndEnsureExistsAsync(command.ProjectId, context.CancellationToken);
        project.AddTasks(command.Tasks.Select(t => t.Name));

        projects.Update(project);
    }
}
```

What happens is as follows: we retrieve the existing project via `projects` repository, add tasks to it and update it in the repository. Again, please notice that the `Update` method is called synchronously — the actual saving of the changes in the database is performed as a part of request pipeline. Also, notice the use of `FindAndEnsureExistsAsync` method. It behaves the same as `FindAsync` method in a repository class when requested object exists and throws an exception if it doesn't.

[^1]: There is also a third type of action called 'operation', which can both modify object and return data. It should only be used out of necessity as it is a violation of CQRS principle. One example of such necessity is using a third party API which doesn't conform to the CQRS pattern.
