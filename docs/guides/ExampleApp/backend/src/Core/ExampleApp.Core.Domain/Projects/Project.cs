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

    public void AddAssignments(IEnumerable<string> taskNames)
    {
        this.assignments.AddRange(taskNames.Select(tn => Assignment.Create(this, tn)));
    }

    public void EditTask(SId<Assignment> taskId, string name)
    {
        assignments.Single(t => t.Id == taskId).Edit(name);
    }

    public void AssignEmployeeToTask(SId<Assignment> taskId, SId<Employee> employeeId)
    {
        assignments.Single(t => t.Id == taskId).AssignEmployee(employeeId);
        DomainEvents.Raise(new UserAssignedToTask(taskId, employeeId));
    }

    public void UnassignEmployeeFromTask(SId<Assignment> taskId)
    {
        assignments.Single(t => t.Id == taskId).UnassignEmployee();
    }

    public void ChangeTaskStatus(SId<Assignment> taskId, Assignment.AssignmentStatus status)
    {
        assignments.Single(t => t.Id == taskId).ChangeStatus(status);
    }
}
