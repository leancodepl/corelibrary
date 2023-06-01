using System.Text.Json.Serialization;
using ExampleApp.Core.Domain.Projects;
using LeanCode.DomainModels.Model;
using LeanCode.Time;

namespace ExampleApp.Core.Domain.Events;

public class UserAssignedToTask : IDomainEvent
{
    public Guid Id { get; private init; }
    public DateTime DateOccurred { get; private init; }

    public SId<Projects.Assignment> TaskId { get; private init; }
    public SId<Employee> UserId { get; private init; }

    public UserAssignedToTask(SId<Projects.Assignment> taskId, SId<Employee> userId)
    {
        Id = Guid.NewGuid();
        DateOccurred = TimeProvider.Now;

        TaskId = taskId;
        UserId = userId;
    }

    [JsonConstructor]
    public UserAssignedToTask(Guid id, DateTime dateOccurred, SId<Projects.Assignment> taskId, SId<Employee> userId)
    {
        Id = id;
        DateOccurred = dateOccurred;

        TaskId = taskId;
        UserId = userId;
    }
}
