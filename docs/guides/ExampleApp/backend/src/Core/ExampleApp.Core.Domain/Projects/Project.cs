using ExampleApp.Core.Domain.Events;
using LeanCode.DomainModels.Model;

namespace ExampleApp.Core.Domain.Projects;

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

    public void AddAssignments(IEnumerable<string> assignmentNames)
    {
        this.assignments.AddRange(assignmentNames.Select(tn => Assignment.Create(this, tn)));
    }

    public void EditAssignment(SId<Assignment> assignmentId, string name)
    {
        assignments.Single(t => t.Id == assignmentId).Edit(name);
    }

    public void AssignEmployeeToAssignment(SId<Assignment> assignmentId, SId<Employee> employeeId)
    {
        assignments.Single(t => t.Id == assignmentId).AssignEmployee(employeeId);
        DomainEvents.Raise(new EmployeeAssignedToAssignment(assignmentId, employeeId));
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
