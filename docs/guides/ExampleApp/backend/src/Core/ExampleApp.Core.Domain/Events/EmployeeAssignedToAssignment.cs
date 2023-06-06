using System.Text.Json.Serialization;
using ExampleApp.Core.Domain.Projects;
using LeanCode.DomainModels.Model;
using LeanCode.Time;

namespace ExampleApp.Core.Domain.Events;

public class EmployeeAssignedToAssignment : IDomainEvent
{
    public Guid Id { get; private init; }
    public DateTime DateOccurred { get; private init; }

    public SId<Assignment> AssignmentId { get; private init; }
    public SId<Employee> EmployeeId { get; private init; }

    public EmployeeAssignedToAssignment(SId<Projects.Assignment> assignmentId, SId<Employee> employeeId)
    {
        Id = Guid.NewGuid();
        DateOccurred = TimeProvider.Now;

        AssignmentId = assignmentId;
        EmployeeId = employeeId;
    }

    [JsonConstructor]
    public EmployeeAssignedToAssignment(
        Guid id,
        DateTime dateOccurred,
        SId<Assignment> assignmentId,
        SId<Employee> employeeId
    )
    {
        Id = id;
        DateOccurred = dateOccurred;

        AssignmentId = assignmentId;
        EmployeeId = employeeId;
    }
}
