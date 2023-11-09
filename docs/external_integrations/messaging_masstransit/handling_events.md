# Handling events

Once an event is raised, it can be handled by a corresponding `IConsumer` to perform the desired action. The default consumer configuration can be customized by overriding the `ConfigureConsumer` method from the `ConsumerDefinition` interface. In the following example, an email is sent to the employee who has been assigned to an assignment:

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| MassTransit | [![NuGet version (MassTransit)](https://img.shields.io/nuget/v/MassTransit.svg?style=flat-square)](https://www.nuget.org/packages/MassTransit/8.1.1/) | `IConsumer` |

```csharp
public class SendEmailToEmployeeOnEmployeeAssignedToAssignment
    : IConsumer<EmployeeAssignedToAssignment>
{
    private readonly IEmailSender emailSender;
    private readonly IRepository<Employee, EmployeeId> employees;
    private readonly IRepository<Assignment, AssignmentId> assignments;

    public SendEmailToEmployeeOnEmployeeAssignedToAssignment(
        IEmailSender emailSender,
        IRepository<Employee, EmployeeId> employees,
        IRepository<Assignment, AssignmentId> assignments)
    {
        this.emailSender = emailSender;
        this.employees = employees;
        this.assignments = assignments;
    }

    public async Task Consume(
        ConsumeContext<EmployeeAssignedToAssignment> context)
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
```

> **Tip:** More about consumers can be found here: [MassTransit Consumers](https://masstransit.io/documentation/concepts/consumers). To read how you can send this email using LeanCode CoreLibrary SendGrid integration visit [here](../../external_integrations/emails_sendgrid/index.md).
