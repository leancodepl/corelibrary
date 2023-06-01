using LeanCode.DomainModels.Model;

namespace ExampleApp.Core.Domain.Projects;

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

    public enum AssignmentStatus
    {
        NotStarted,
        InProgress,
        Finished,
    }
}
