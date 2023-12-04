# Avoid committing transactions in handlers

> **Tip:** Before delving into this section, it's highly recommended to explore the [CQRS], [Domain] and [MassTransit] sections.

**⚠️ Directly commiting transactions in [command]/[operation] handlers poses a challenge, potentially causing inconsistencies between [events] and the associated entities.**

Let's consider following pipeline configuration:

```csharp
    protected override void ConfigureApp(IApplicationBuilder app)
    {
        // . . .
        app.UseEndpoints(endpoints =>
            {
                endpoints.MapRemoteCqrs(
                    "/api",
                    cqrs =>
                    {
                        cqrs.Commands = c =>
                            c.CQRSTrace()
                            .Secure()
                            .Validate()
                            .CommitTransaction<CoreDbContext>()
                            .PublishEvents();

                        cqrs.Queries = c =>
                            c.CQRSTrace()
                            .Secure();

                        cqrs.Operations = c =>
                            c.CQRSTrace()
                            .Secure()
                            .CommitTransaction<CoreDbContext>()
                            .PublishEvents();
                    }
                );
            });
    }
```

The code snippet above involves a `CommitDatabaseTransactionMiddleware<TDbContext>` middleware which is invoked by the `CommitTransaction<CoreDbContext>()` method. This middleware executes subsequent middlewares and then calls the `SaveChangesAsync(...)` method on `CoreDbContext` consolidating all changes made within the pipeline into a single transaction:

```csharp
public class CommitDatabaseTransactionMiddleware<TDbContext>
    where TDbContext : DbContext
{
    private readonly RequestDelegate next;

    public CommitDatabaseTransactionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, TDbContext dbContext)
    {
        await next(httpContext);
        await dbContext.SaveChangesAsync(httpContext.RequestAborted);
    }
}
```

Take, for instance, an example where we wish to send an email to an employee upon the occurrence of the `EmployeeAssignedToAssignment` event:

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

    // . . .
}
```

Now, let's imagine using this method in a [command] handler:

```csharp
public class AssignEmployeeToAssignmentCH
    : ICommandHandler<AssignEmployeeToAssignment>
{
    private readonly IRepository<Project, ProjectId> projects;
    private readonly CoreDbContext dbContext;

    public AssignEmployeeToAssignmentCH(
        IRepository<Project, ProjectId> projects,
        CoreDbContext dbContext)
    {
        this.projects = projects;
        this.dbContext = dbContext;
    }

    public Task ExecuteAsync(HttpContext context, UpdateProjectName command)
    {
        var project = await projects.FindAndEnsureExistsAsync(
            ProjectId.Parse(command.ProjectId),
            context.RequestAborted);

        project.AssignEmployeeToAssignment(
            AssignmentId.Parse(command.AssignmentId),
            EmployeeId.Parse(command.EmployeeId));

        projects.Update(project);

        // Directly committing the transaction in the command handler,
        // which should be avoided.
        await dbContext.SaveChangesAsync(context.RequestAborted)
    }
}
```

In our pipeline configuration, [events] are published after the [command] handler is executed. This implies that changes on `CoreDbContext` will be committed before [events] are published. In LeanCode Corelibrary, we utilize [MassTransit] for message broker interaction, employing the [transactional outbox](https://masstransit.io/documentation/patterns/transactional-outbox) concept. This ensures that messages are committed to the database before being available to message brokers after publication and the invocation of the `SaveChangesAsync` method on CoreDbContext.

In our scenario, if the database fails after successfully saving changes to the project, the `EmployeeAssignedToAssignment` message won't be saved, leading to it being unavailable to message brokers and not sent.

Conversely, removing `SaveChangesAsync` from the [command] handler would result in both messages and project changes being committed in a single transaction. In the event of a database failure, neither project changes nor messages would be saved or sent, providing clients with information about the request failure without unintended side effects. This is why it's not recommended to not commit transactions directly in [command]/[operation] handlers.

> **Tip:** To read more about LeanCode Corelibrary MassTransit integtation visit [here](../../external_integrations/messaging_masstransit/index.md).

[CQRS]: ../index.md
[Domain]: ../../domain/index.md
[MassTransit]: ../../external_integrations/messaging_masstransit/index.md
[events]: ../../domain/domain_event/index.md
[command]: ../command/index.md
[operation]: ../operation/index.md